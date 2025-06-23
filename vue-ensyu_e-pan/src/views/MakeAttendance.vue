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
</style>

<template>
  <h1>シフト調整</h1>
  <div v-if="loading">データを読み込み中...</div>
  <div v-if="apiError">{{ apiError }}</div>

  <div v-if="!loading && !apiError">
    <div class="date-navigation">
      <button @click="changeDay(-1)">前の日へ</button>
      <h2>{{ formattedDate }}</h2>
      <button @click="changeDay(1)">次の日へ</button>
    </div>
    <p>{{ storeName }}</p>

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
          <tr><!--上段: 希望シフト-->
            <th class="event-col" rowspan="2">
              {{ schedule.userName }}
            </th>
            <td rowspan="2">¥{{ schedule.hourlyWage }}</td>
            <td rowspan="2"><input type="text" v-model="schedule.workRollName"></td>
            <td>{{ schedule.hopeStart }}</td>
            <td>{{ schedule.hopeEnd }}</td>
            <template v-for="(cell, cellIndex) in calculateRow(schedule.hopeStart, schedule.hopeEnd)" :key="cellIndex">
              <td v-if="cell.type === 'event'" :colspan="cell.colspan" class="event-block">希望</td>
              <td v-if="cell.type === 'empty'" class="empty">&nbsp;</td>
            </template>
          </tr>
          <tr><!--下段: 予定シフト-->
            <td>
              <input type="time" v-model="schedule.plannedStart">
              <div v-if="errors[index]?.start" class="error-message">{{ errors[index].start }}</div>
            </td>
            <td>
              <input type="time" v-model="schedule.plannedEnd">
              <div v-if="errors[index]?.end" class="error-message">{{ errors[index].end }}</div>
            </td>
            <template v-for="(cell, cellIndex) in calculateRow(schedule.plannedStart, schedule.plannedEnd)" :key="cellIndex">
              <td v-if="cell.type === 'event'" :colspan="cell.colspan" class="event-block">予定</td>
              <td v-if="cell.type === 'empty'" class="empty">&nbsp;</td>
            </template>
          </tr>
        </template>
      </tbody>
    </table>
    <div style="text-align: right; margin-top: 1rem;">
      <button @click="saveChanges" :disabled="!isDirty">保存</button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue';
import { getShiftsByDate, updateSchedulesBulk } from '../services/api.js';
import { isEqual } from 'lodash-es';

// --- State ---
const currentDate = ref(new Date());
const storeName = ref('');
const schedules = ref([]);
const originalSchedules = ref([]); // To track changes
const isDirty = ref(false); // To track unsaved changes
const errors = ref([]);
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
    originalSchedules.value = JSON.parse(JSON.stringify(filteredSchedules));
    storeName.value = data.storeName;
    isDirty.value = false;
  } catch (error) {
    apiError.value = 'シフトデータの読み込みに失敗しました。';
    console.error(error);
  } finally {
    loading.value = false;
  }
};

const changeDay = (days) => {
  if (isDirty.value) {
    if (!window.confirm("内容が保存されていません。ページを移動しますか？")) {
      return; // Stop navigation if user cancels
    }
  }
  const newDate = new Date(currentDate.value);
  newDate.setDate(newDate.getDate() + days);
  currentDate.value = newDate;
  fetchShifts(currentDate.value);
};

const saveChanges = async () => {
  try {
    await updateSchedulesBulk(schedules.value);
    originalSchedules.value = JSON.parse(JSON.stringify(schedules.value));
    isDirty.value = false;
    alert('保存しました。');
  } catch (error) {
    alert('保存に失敗しました。');
    console.error('Failed to save changes:', error);
  }
};

// --- Watchers ---
watch(schedules, (newSchedules) => {
  // Input validation
  errors.value = newSchedules.map(schedule => {
    const eventErrors = { start: null, end: null };
    const startMin = getMinutes(schedule.plannedStart);
    const endMin = getMinutes(schedule.plannedEnd);
    const minStart = getMinutes(`${String(startHour).padStart(2, '0')}:00`);
    if (startMin !== -1 && startMin < minStart) {
      eventErrors.start = `${startHour}:00以降にしてください。`;
    }
    if (startMin !== -1 && endMin !== -1 && startMin >= endMin) {
      eventErrors.end = '退勤は出勤より後にしてください。';
    }
    return eventErrors;
  });

  // Set dirty flag if data has changed from original
  if (originalSchedules.value.length > 0) {
    isDirty.value = !isEqual(newSchedules, originalSchedules.value);
  }

}, { deep: true });

// --- Lifecycle Hooks ---
onMounted(() => {
  currentDate.value = new Date(2024, 5, 23); // For consistent mock data
  fetchShifts(currentDate.value);
});
</script>


