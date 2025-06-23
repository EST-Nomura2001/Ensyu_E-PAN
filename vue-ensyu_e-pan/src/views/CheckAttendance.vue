<style scoped>
  body {
    font-family: sans-serif;
    padding: 20px;
  }
  table {
    border-collapse: collapse;
    width: 100%;
    table-layout: fixed;
  }
  th, td {
  
    border: 1px solid #ccc;
    padding: 6px;
    text-align: center;
    font-size: 12px;
  }
  th{
      width: 80px;
  }
  th.time-header {
    text-align: left;
    width: 60px;
    font-size: 10px;
  }
  th.event-col {
    background: #f0f0f0;
    width: 120px;
  }
  td.empty {
    background-color: #f8f8f8;
    color: #aaa;
  }
  td.event-block {
    background-color: #d0f0c0;
    font-weight: bold;
  }
  input[type="time"], input[type="text"] {
    width: 70px;
  }
  .error-message {
    color: red;
    font-size: 10px;
    text-align: left;
  }
  .date-navigation {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;
  }
  .date-navigation h2 {
    margin: 0;
  }

/* 月別カレンダーの表示です */
/* Calendar Styles */
.calendar-container {
  margin-bottom: 1rem;
  border: 1px solid #ccc;
  padding: 5px;
  border-radius: 5px;
  max-width: 300px; /* Set a max-width for better layout */
  margin-left: auto;
  margin-right: auto;
}
.calendar-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 5px;
}
.calendar-header h2 {
  margin: 0;
  font-size: 0.9em;
}
.calendar-header button {
    padding: 1px 3px;
    font-size: 0.8em;
}
.calendar-table {
  width: 100%;
  border-collapse: collapse;
}
.calendar-table th, .calendar-table td {
  border: 1px solid #eee;
  padding: 2px;
  text-align: center;
  font-size: 10px;
  vertical-align: middle;
  width: 14.2%;
}
.calendar-table td {
  cursor: pointer;
  height: 2.4em;
}
.calendar-table td:hover {
  background-color: #f0f0f0;
}
.not-current-month {
  color: #aaa;
}
.selected-day {
  background-color: #d0f0c0;
  font-weight: bold;
}
.sunday {
  color: red;
}
.saturday {
  color: blue;
}
/* 月別カレンダーの表示です */
</style>

<template>
  <h1>シフト確認</h1>
  
  <div v-if="loading">データを読み込み中...</div>
  <div v-if="apiError">{{ apiError }}</div>

  <div v-if="!loading && !apiError">
    <div class="calendar-container">
      <div class="calendar-header">
        <button @click="changeMonth(-1)">&lt; 前の月</button>
        <h2>{{ calendarYear }}年 {{ calendarMonth }}月</h2>
        <button @click="changeMonth(1)">次の月 &gt;</button>
      </div>
      <table class="calendar-table">
        <thead>
          <tr>
            <th v-for="(day, index) in weekDays" :key="index" :class="{ 'sunday': index === 0, 'saturday': index === 6 }">{{ day }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(week, weekIndex) in calendarGrid" :key="weekIndex">
            <td v-for="day in week" :key="day.date.getTime()" @click="selectDate(day)"
                :class="{ 
                  'not-current-month': !day.isCurrentMonth, 
                  'selected-day': isSelected(day), 
                  'sunday': day.date.getDay() === 0, 
                  'saturday': day.date.getDay() === 6 
                }">
              {{ day.day }}
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="date-navigation">
      <button @click="changeDay(-1)">前の日へ</button>
      <h2>{{ formattedDate }}</h2>
      <button @click="changeDay(1)">次の日へ</button>
    </div>
    <div>
      <p>{{ storeName }}</p>
      <p>ステータス：保存済み <button @click="goToMakeAttendance">編集ページへ</button></p>　<!--日付をクエリパラメータで編集ページに送っている-->
    </div>
    <table id="timeTable">
      <thead>
        <tr>
          <th>名前</th>
          <th>時給</th>
          <th>担当業務</th>
          <th>出勤時間</th>
          <th>退勤時間</th>
          <th v-for="time in timeHeaders" :key="time" class="time-header" colspan="2">{{ time }}</th>
        </tr>
      </thead>
      <tbody>
        <template v-for="(schedule, index) in schedules" :key="schedule.scheduleId">
          <tr>
            <th class="event-col">
              {{ schedule.userName }}
            </th>
            <td >¥{{ schedule.hourlyWage }}</td>
            <td >{{ schedule.workRollName }}</td>
            <td>{{ schedule.plannedStart }}</td>
            <td>{{ schedule.plannedEnd }}</td>
            <template v-for="(cell, cellIndex) in calculateRow(schedule.plannedStart, schedule.plannedEnd)" :key="cellIndex">
              <td v-if="cell.type === 'event'" :colspan="cell.colspan" class="event-block">&nbsp;</td>
              <td v-if="cell.type === 'empty'" class="empty">&nbsp;</td>
            </template>
          </tr>
        </template>
      </tbody>
    </table>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { getShiftsByDate } from '../services/api.js';
import { useRouter } from 'vue-router';

const router = useRouter();

// --- State ---
const currentDate = ref(new Date());
const calendarDate = ref(new Date());
const storeName = ref('');
const schedules = ref([]);
const loading = ref(true);
const apiError = ref(null);

// --- Constants ---
const startHour = 9;
const endHour = 24;
const intervalMinutes = 30;

// --- Computed Properties ---
const formattedDate = computed(() => {
  const month = currentDate.value.getMonth() + 1;
  const day = currentDate.value.getDate();
  return `${month}月${day}日`;
});

const calendarYear = computed(() => calendarDate.value.getFullYear());
const calendarMonth = computed(() => calendarDate.value.getMonth() + 1);
const weekDays = ['日', '月', '火', '水', '木', '金', '土'];

const calendarGrid = computed(() => {
  const year = calendarYear.value;
  const month = calendarMonth.value - 1; // JS month is 0-indexed

  const grid = [];
  const startDate = new Date(year, month, 1);
  startDate.setDate(startDate.getDate() - startDate.getDay()); // Start from Sunday of the first week

  for (let i = 0; i < 6; i++) { // 6 weeks for consistency
    const week = [];
    for (let j = 0; j < 7; j++) {
      week.push({
        day: startDate.getDate(),
        date: new Date(startDate),
        isCurrentMonth: startDate.getMonth() === month,
      });
      startDate.setDate(startDate.getDate() + 1);
    }
    grid.push(week);
  }
  return grid;
});

const timeHeaders = computed(() => {
  const headers = [];
  for (let h = startHour; h < endHour; h++) {
    headers.push(`${String(h).padStart(2, "0")}:00～`);
  }
  return headers;
});

const timeSlots = computed(() => {
  const slots = [];
  for (let h = startHour; h < endHour; h++) {
    for (let m = 0; m < 60; m += intervalMinutes) {
      slots.push(`${String(h).padStart(2, "0")}:${String(m).padStart(2, "0")}～`);
    }
  }
  return slots;
});

// --- Functions ---
function changeMonth(amount) {
  const newDate = new Date(calendarDate.value);
  newDate.setMonth(newDate.getMonth() + amount, 1);
  calendarDate.value = newDate;
}

function selectDate(day) {
    if (!day.date) return;
    currentDate.value = day.date;
    if (!day.isCurrentMonth) {
        calendarDate.value = day.date;
    }
    fetchShifts(currentDate.value);
}

function isSelected(day) {
  if (!day.date) return false;
  return day.date.getFullYear() === currentDate.value.getFullYear() &&
         day.date.getMonth() === currentDate.value.getMonth() &&
         day.date.getDate() === currentDate.value.getDate();
}

function getMinutes(timeStr) {
  if (!timeStr || typeof timeStr !== 'string' || !timeStr.includes(':')) return -1;
  const [h, m] = timeStr.split(":").map(Number);
  if (isNaN(h) || isNaN(m)) return -1;
  return h * 60 + m;
}

function calculateRow(startTime, endTime) {
  const cells = [];
  const startMin = getMinutes(startTime);
  const endMin = getMinutes(endTime);

  if (startMin === -1 || endMin === -1 || startMin >= endMin) {
    return Array(timeSlots.value.length).fill(0).map(() => ({ type: 'empty' }));
  }

  for (let i = 0; i < timeSlots.value.length; ) {
    const tMin = getMinutes(timeSlots.value[i].substring(0, 5));
    if (tMin >= endMin) {
      cells.push({ type: 'empty' });
      i++;
      continue;
    }
    if (tMin >= startMin) {
      let span = 0;
      for (let j = i; j < timeSlots.value.length; j++) {
        const checkMin = getMinutes(timeSlots.value[j].substring(0, 5));
        if (checkMin < endMin) span++;
        else break;
      }
      if (span > 0) {
        cells.push({ type: 'event', colspan: span });
        i += span;
      } else {
        cells.push({ type: 'empty' });
        i++;
      }
    } else {
      cells.push({ type: 'empty' });
      i++;
    }
  }
  return cells;
}

const fetchShifts = async (date) => {
  loading.value = true;
  apiError.value = null;
  try {
    const data = await getShiftsByDate(date);
    const filteredSchedules = data.schedules.filter(s => s.hopeStart || s.plannedStart);
    schedules.value = JSON.parse(JSON.stringify(filteredSchedules));
    storeName.value = data.storeName;
  } catch (error) {
    apiError.value = 'シフトデータの読み込みに失敗しました。';
    console.error(error);
  } finally {
    loading.value = false;
  }
};

const changeDay = (days) => {
  const newDate = new Date(currentDate.value);
  newDate.setDate(newDate.getDate() + days);
  currentDate.value = newDate;
  calendarDate.value = newDate;
  fetchShifts(currentDate.value);
};

const goToMakeAttendance = () => {
  const yyyy = currentDate.value.getFullYear();
  const mm = String(currentDate.value.getMonth() + 1).padStart(2, '0');
  const dd = String(currentDate.value.getDate()).padStart(2, '0');
  const dateString = `${yyyy}-${mm}-${dd}`;
  router.push({ name: 'Make-Attendance', query: { date: dateString } });
};

// --- Lifecycle Hooks ---
onMounted(() => {
  currentDate.value = new Date(2024, 5, 23); // For consistent mock data
  calendarDate.value = new Date(2024, 5, 23);
  fetchShifts(currentDate.value);
});
</script>


