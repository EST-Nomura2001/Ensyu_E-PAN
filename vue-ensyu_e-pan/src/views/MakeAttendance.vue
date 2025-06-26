<!-- 担当業務の更新と時給の表示が未実装 -->

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
    width: 30px;
    padding-left: 3px;
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
    background-color: #b0d8a0;
    font-weight: bold;
  }
  td.event-block-dark {
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
  .calendar-navi{
    Text-align: center
  }
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
  h1 {
    writing-mode: horizontal-tb !important; /* 横書きを強制 */
    text-align: left !important;            /* 左寄せ */
    margin: 0 0 1rem 0;
    font-size: 2em;
  }
</style>

<template>
  <CommonHeader />
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
          <th>取消</th>
          <th v-for="h in (endHour - startHour)" :key="h" class="time-header" :colspan="6">{{ String(startHour + h - 1).padStart(2, '0') }}:00</th>
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
            <td></td>
            <template v-for="(cell, cellIndex) in calculateRow(schedule.hopeStart, schedule.hopeEnd)" :key="cellIndex">
              <td v-if="cell.type === 'event'" :colspan="cell.colspan" class="event-block">希望</td>
              <td v-if="cell.type === 'empty'" class="empty">&nbsp;</td>
            </template>
          </tr>
          <tr><!--下段: 予定シフト-->
            <td>
              <input type="time" v-model="schedule.plannedStart" @input="validatePlannedShift(index)" step="600">
              <div v-if="errors[index]?.start" class="error-message">{{ errors[index].start }}</div>
            </td>
            <td>
              <input type="time" v-model="schedule.plannedEnd" @input="validatePlannedShift(index)" step="600">
              <div v-if="errors[index]?.end" class="error-message">{{ errors[index].end }}</div>
            </td>
            <td>
              <button type="button" @click="resetPlannedShift(index)">取消</button>
            </td>
            <template v-for="(cell, cellIndex) in calculateRow(schedule.plannedStart, schedule.plannedEnd)" :key="cellIndex">
              <td v-if="cell.type === 'event'" :colspan="cell.colspan" class="event-block-dark">予定</td>
              <td v-if="cell.type === 'empty'" class="empty">&nbsp;</td>
            </template>
          </tr>
        </template>
      </tbody>
    </table>
    <div style="text-align: left; margin-top: 1rem;">
      <button @click="saveChanges" :disabled="!isDirty">保存</button>
    </div>

    <!-- カレンダーを表の下中央に配置 -->
    <div style="display: flex; justify-content: center; margin-top: 1rem;">
      <div>
        <span class="calendar-navi">ジャンプしたい日付をクリックしてください。</span>
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
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue';
import { getDateSchedulesByDate, updateSchedulesBulk } from '../services/api.js';
import { isEqual } from 'lodash-es';
import { useRoute, useRouter } from 'vue-router';
import axios from 'axios';

//ヘッダー用
import CommonHeader from '../components/CommonHeader.vue';

const route = useRoute();
const router = useRouter();

// --- State ---
const currentDate = ref(new Date());
const calendarDate = ref(new Date());
const storeName = ref('');
const schedules = ref([]);
const originalSchedules = ref([]); // To track changes
const isDirty = ref(false); // To track unsaved changes
const errors = ref([]);
const loading = ref(true);
const apiError = ref(null);

// --- Constants ---
const startHour = 9;
const endHour = 22;
const intervalMinutes = 10;

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
  const month = calendarMonth.value - 1;

  const grid = [];
  const startDate = new Date(year, month, 1);
  startDate.setDate(startDate.getDate() - startDate.getDay());

  for (let i = 0; i < 6; i++) {
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
    // 日付を "YYYY-MM-DD" 形式に変換
    const yyyy = date.getFullYear();
    const mm = String(date.getMonth() + 1).padStart(2, '0');
    const dd = String(date.getDate()).padStart(2, '0');
    const dateString = `${yyyy}-${mm}-${dd}`;

    const data = await getDateSchedulesByDate(dateString);
    console.log('APIレスポンス', data);
    const rawSchedules = Array.isArray(data) ? data : (data.$values || []);
    schedules.value = rawSchedules.map(ds => {
      // userDateShiftsが配列であることを確認
      const userDateShift = ds.userDateShifts?.$values?.[0];
      const dateSchedule = userDateShift?.dateSchedule || {};

      return {
        scheduleId: dateSchedule.id,
        userId: ds.user_Id,
        userName: ds.userName || '',
        hourlyWage: '', // 必要ならAPIで返すようにする
        workRollName: dateSchedule.workRollName || '',
        plannedStart: dateSchedule.p_Start_WorkTime ? dateSchedule.p_Start_WorkTime.substring(11, 16) : '',
        plannedEnd: dateSchedule.p_End_WorkTime ? dateSchedule.p_End_WorkTime.substring(11, 16) : '',
        hopeStart: dateSchedule.u_Start_WorkTime ? dateSchedule.u_Start_WorkTime.substring(11, 16) : '',
        hopeEnd: dateSchedule.u_End_WorkTime ? dateSchedule.u_End_WorkTime.substring(11, 16) : ''
      };
    });
    originalSchedules.value = JSON.parse(JSON.stringify(schedules.value));
    storeName.value = rawSchedules.length > 0 && rawSchedules[0].storeName ? rawSchedules[0].storeName : '';
    isDirty.value = false;
  } catch (error) {
    apiError.value = 'シフトデータの読み込みに失敗しました。';
    console.error(error);
  } finally {
    loading.value = false;
  }
};

const changeMonth = (amount) => {
  const newDate = new Date(calendarDate.value);
  newDate.setMonth(newDate.getMonth() + amount, 1);
  calendarDate.value = newDate;
};

const selectDate = (day) => {
  if (isDirty.value) {
    if (!window.confirm("内容が保存されていません。ページを移動しますか？")) {
      return;
    }
  }
  if (!day.date) return;
  currentDate.value = day.date;
  if (!day.isCurrentMonth) {
    calendarDate.value = day.date;
  }
  fetchShifts(currentDate.value);
};

function isSelected(day) {
  if (!day.date) return false;
  return day.date.getFullYear() === currentDate.value.getFullYear() &&
         day.date.getMonth() === currentDate.value.getMonth() &&
         day.date.getDate() === currentDate.value.getDate();
}

const changeDay = (days) => {
  if (isDirty.value) {
    if (!window.confirm("内容が保存されていません。ページを移動しますか？")) {
      return; // Stop navigation if user cancels
    }
  }
  const newDate = new Date(currentDate.value);
  newDate.setDate(newDate.getDate() + days);
  currentDate.value = newDate;
  calendarDate.value = newDate;
  fetchShifts(currentDate.value);
};

const saveChanges = async () => {
  try {
    const yyyy = currentDate.value.getFullYear();
    const mm = String(currentDate.value.getMonth() + 1).padStart(2, '0');
    const dd = String(currentDate.value.getDate()).padStart(2, '0');
    const dateString = `${yyyy}-${mm}-${dd}`;

    // 全員分まとめてPUT
    await Promise.all(schedules.value.map(async (schedule) => {
      // 入力が空の場合はスキップ
      if (!schedule.plannedStart || !schedule.plannedEnd) return;
      // ISO形式に変換
      const toISO = (date, time) => `${date}T${time}:00.000Z`;
      const body = {
        targetDate: toISO(dateString, schedule.plannedStart), // targetDateは開始時刻で送信
        p_Start_WorkTime: toISO(dateString, schedule.plannedStart),
        p_End_WorkTime: toISO(dateString, schedule.plannedEnd)
      };
      await axios.put(
        `http://localhost:5011/api/Attendance/user/${schedule.userId}/schedule-plan`,
        body
      );
    }));
    originalSchedules.value = JSON.parse(JSON.stringify(schedules.value));
    isDirty.value = false;
    alert('保存しました。');
    // 再読込
    fetchShifts(currentDate.value);
  } catch (error) {
    alert('保存に失敗しました。');
    console.error('Failed to save changes:', error);
  }
};

// KibouForm.vueのバリデーションを参考にした関数
function validatePlannedShift(idx) {
  const schedule = schedules.value[idx];
  errors.value[idx] = { start: '', end: '' };
  // 両方未入力はOK
  if (!schedule.plannedStart && !schedule.plannedEnd) return;
  // 片方だけ入力はNG
  if (!schedule.plannedStart || !schedule.plannedEnd) {
    errors.value[idx].start = '出勤・退勤時間は両方入力、または両方未入力にしてください。';
    errors.value[idx].end = '';
    return;
  }
  // 出勤時間9:00未満はNG
  if (schedule.plannedStart < '09:00') {
    errors.value[idx].start = '出勤時間は9:00以降にしてください。';
  }
  // 退勤時間24:00超はNG
  if (schedule.plannedEnd > '24:00') {
    errors.value[idx].end = '退勤時間は24:00までにしてください。';
  }
  // 退勤時間が出勤時間以前はNG
  if (schedule.plannedEnd && schedule.plannedStart && schedule.plannedEnd <= schedule.plannedStart) {
    errors.value[idx].end = '退勤時間は出勤時間より後にしてください。';
  }
}

function resetPlannedShift(idx) {
  schedules.value[idx].plannedStart = '';
  schedules.value[idx].plannedEnd = '';
  validatePlannedShift(idx);
}

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
  // クエリから日付取得
  let dateParam = route.query.date;
  let initialDate;
  if (dateParam && typeof dateParam === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(dateParam)) {
    // "YYYY-MM-DD" 形式
    const [yyyy, mm, dd] = dateParam.split('-').map(Number);
    initialDate = new Date(yyyy, mm - 1, dd);
  } else {
    // クエリがなければ本日
    initialDate = new Date();
  }
  currentDate.value = initialDate;
  calendarDate.value = new Date(initialDate);
  fetchShifts(currentDate.value);
});
</script>


