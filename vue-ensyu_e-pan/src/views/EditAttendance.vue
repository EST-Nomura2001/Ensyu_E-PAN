<!-- ストアの絞り込みなし
 担当業務の変更なし -->
<template>
  <CommonHeader />
  <template v-if="!checkedAuth">
    <div>認証確認中...</div>
  </template>
  <template v-else-if="!isAdmin">
    <div style="color: red; font-size: 1.2em; margin: 2em;">権限がありません</div>
  </template>
  <template v-else>
    <div class="edit-attendance">
      <h1>勤怠編集</h1>
      <h2>{{ formattedDayShiftDate }}の勤怠情報</h2>
      <router-link :to="{ name: 'Record-Attendance', query: { date: recordAttendanceMonthQuery } }">
        {{ formattedMonth }}の勤怠一覧に戻る
      </router-link>
      <div v-if="fetchError" class="fetch-error-message">データを取得できませんでした</div>
      <div>
        <div class="edit-controls">
          <button v-if="!isEditing" @click="startEdit">編集</button>
          <button v-else @click="saveEdit">保存</button>
          <button v-if="isEditing" @click="cancelEdit">キャンセル</button>
        </div>
        <table>
          <thead>
            <tr>
              <th>名前</th>
              <!--<th>時給(円)</th>-->
              <th>人件費(円)</th>
              <th>勤務時間(h)</th>
              <th>うち深夜勤務時間(h)</th> <!--要相談-->
              <th>出勤時間</th>
              <th>退勤時間</th>
              <th>休憩入り時間</th>
              <th>休憩戻り時間</th>
              <th>担当業務</th>
            </tr>
          </thead>
          <tbody>
            <tr class="total-row">
              <td>合計</td>
              <!--<td class="slash-cell"></td>-->
              <td>{{ totalLaborCost }}</td>
              <td>{{ totalWorkTime }}</td>
              <td class="slash-cell"></td>
              <td class="slash-cell"></td>
              <td class="slash-cell"></td>
              <td class="slash-cell"></td>
              <td class="slash-cell"></td>
              <td class="slash-cell"></td>
            </tr>
            <tr v-for="(row, idx) in tableData" :key="idx">
              <td>{{ row.name }}</td>
              <!--<td>{{ row.wage }}</td>-->
              <td>{{ row.laborCost }}</td>
              <td>{{ row.workTime }}</td>
              <td>{{ row.nightWorkTime }}</td>
              <td v-if="isEditing">
                <input type="time" v-model="editRows[idx].startTime" style="width: 90px;" />
                <div v-if="errors[idx] && errors[idx].start" class="error-message">{{ errors[idx].start }}</div>
              </td>
              <td v-else>{{ row.startTime }}</td>
              <td v-if="isEditing">
                <input type="time" v-model="editRows[idx].endTime" style="width: 90px;" />
                <div v-if="errors[idx] && errors[idx].end" class="error-message">{{ errors[idx].end }}</div>
              </td>
              <td v-else>{{ row.endTime }}</td>
              <td v-if="isEditing">
                <input type="time" v-model="editRows[idx].breakInTime" style="width: 90px;" />
                <div v-if="errors[idx] && errors[idx].breakIn" class="error-message">{{ errors[idx].breakIn }}</div>
              </td>
              <td v-else>{{ row.breakInTime }}</td>
              <td v-if="isEditing">
                <input type="time" v-model="editRows[idx].breakOutTime" style="width: 90px;" />
                <div v-if="errors[idx] && errors[idx].breakOut" class="error-message">{{ errors[idx].breakOut }}</div>
              </td>
              <td v-else>{{ row.breakOutTime }}</td>
              <td>{{ row.task }}</td>
            </tr>
          </tbody>
        </table>
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
    </div>
  </template>
</template>

<script>
import { getAttendanceByDateStore, updateAttendanceByDateStore } from '../services/api';
import axios from 'axios';

//ヘッダー用
import CommonHeader from '../components/CommonHeader.vue';


  export default {
  name: 'EditAttendance',
  data() {
    return {
      tableData: [],
      isEditing: false,
      editRows: [],
      errors: [],
      totalLaborCost: '',
      totalWorkTime: '',
      totalNightWorkTime: '',
      storeName: '',
      fetchError: false,
      dayShiftDate: '',
      calendarDate: new Date(),
      isAdmin: false,
      checkedAuth: false,
    };
  },
  async mounted() {
    // 権限チェック
    this.isAdmin = sessionStorage.getItem('isAdmin') === 'true';
    this.checkedAuth = true;
    if (!this.isAdmin) return;
    // sessionStorageの内容をコンソールに出力
    console.log('userId:', sessionStorage.getItem('userId'));
    console.log('userName:', sessionStorage.getItem('userName'));
    console.log('storeId:', sessionStorage.getItem('storeId'));
    await this.fetchAttendance();
  },
  methods: {
    async fetchAttendance() {
      try {
        // today（日付）を決定
        let today = this.$route.params.date || this.$route.query.date;
        if (!today) {
          const now = new Date();
          today = now.toISOString().slice(0, 10); // 'YYYY-MM-DD'
        }
        // APIへ直接リクエスト
        const url = `http://localhost:5011/api/Attendance/DateSchedules/${today}`;
        const res = await axios.get(url);
        // レスポンスをtableData用に整形
        this.tableData = [];
        let apiTotalLaborCost = 0;
        let apiTotalWorkTime = '';
        (res.data || []).forEach(dayShift => {
          if (dayShift.sum_TotalCost != null) {
            apiTotalLaborCost += Number(dayShift.sum_TotalCost);
          }
          if (dayShift.sum_WorkTime) {
            // 1日分しかない場合はそのまま、複数日なら合算（現状1日分想定）
            apiTotalWorkTime = dayShift.sum_WorkTime;
          }
          (dayShift.dateSchedules || []).forEach(ds => {
            this.tableData.push({
              name: ds.userName || '',
              userId: ds.userId,
              scheduleId: ds.id,
              workRollId: ds.workRollId,
              dayShiftId: ds.dayShiftId,
              wage: '', // 必要なら追加
              laborCost: ds.t_DayPrice != null ? Number(ds.t_DayPrice).toLocaleString() : '',
              workTime: ds.t_WorkTime_All ? this.timeStrToHourDecimal(ds.t_WorkTime_All) : '',
              nightWorkTime: ds.t_WorkTime_N ? this.timeStrToHourDecimal(ds.t_WorkTime_N) : '',
              startTime: ds.start_WorkTime ? ds.start_WorkTime.slice(11, 16) : '',
              endTime: ds.end_WorkTime ? ds.end_WorkTime.slice(11, 16) : '',
              breakInTime: ds.start_BreakTime ? ds.start_BreakTime.slice(11, 16) : '',
              breakOutTime: ds.end_BreakTime ? ds.end_BreakTime.slice(11, 16) : '',
              task: ds.workRollName || '',
            });
          });
        });
        this.totalLaborCost = apiTotalLaborCost.toLocaleString();
        this.totalWorkTime = apiTotalWorkTime ? this.timeStrToHourDecimal(apiTotalWorkTime) : '';
        this.totalNightWorkTime = ''; // 必要なら合計ロジックを追加
        this.storeName = '';
        this.dayShiftDate = today;
        this.fetchError = false;
      } catch (e) {
        this.fetchError = true;
        // ↓↓↓ デモデータ（後で削除してください）↓↓↓
        this.tableData = [
          {
            name: '田中 太郎', wage: 1200, laborCost: 9600, workTime: '8:00', nightWorkTime: '2:00', startTime: '09:00', endTime: '17:00', task: 'レジ', breakInTime: '12:00', breakOutTime: '13:00',
          },
          {
            name: '鈴木 花子', wage: 1100, laborCost: 8800, workTime: '8:00', nightWorkTime: '0:00', startTime: '10:00', endTime: '18:00', task: '品出し', breakInTime: '12:00', breakOutTime: '13:00',
          },
          {
            name: '佐藤 次郎', wage: 1300, laborCost: 10400, workTime: '8:00', nightWorkTime: '1:00', startTime: '13:00', endTime: '21:00', task: '清掃', breakInTime: '12:00', breakOutTime: '13:00',
          },
        ];
        this.totalLaborCost = 28800;
        this.totalWorkTime = '24:00';
        this.totalNightWorkTime = '3:00';
        this.storeName = 'デモ店舗';
        this.dayShiftDate = '2024-06-24';
        // ↑↑↑ デモデータここまで ↑↑↑
      }
    },
    async saveEdit() {
      if (!this.validateAll()) {
        alert('入力内容に誤りがあります。修正してください。');
        return;
      }
      try {
        // 編集日付
        const date = this.dayShiftDate;
        // 全行PUT
        for (let i = 0; i < this.editRows.length; i++) {
          const edit = this.editRows[i];
          const original = this.tableData[i];
          // 日付部分
          const ymd = date.split('T')[0];
          // 時刻をYYYY-MM-DDTHH:mm:00形式に
          const toIso = (t) => t ? `${ymd}T${t}:00` : null;
          const body = {
            id: original.scheduleId,
            user_Id: original.userId,
            work_Roll_Id: original.workRollId,
            day_Shift_Id: original.dayShiftId,
            start_WorkTime: toIso(edit.startTime),
            end_WorkTime: toIso(edit.endTime),
            start_BreakTime: toIso(edit.breakInTime),
            end_BreakTime: toIso(edit.breakOutTime)
          };
          const url = `http://localhost:5011/api/Attendance/users/${original.userId}/schedules/${original.scheduleId}`;
          await axios.put(url, body);
        }
        await this.fetchAttendance();
        this.isEditing = false;
        alert('保存しました');
      } catch (e) {
        alert('保存に失敗しました');
      }
    },
    getMinutes(timeStr) {
      if (!timeStr || typeof timeStr !== 'string' || !timeStr.includes(':')) return -1;
      const [h, m] = timeStr.split(":").map(Number);
      if (isNaN(h) || isNaN(m)) return -1;
      return h * 60 + m;
    },
    validateRow(row) {
      const startHour = 9;
      const eventErrors = { start: null, end: null, breakIn: null, breakOut: null };
      const startMin = this.getMinutes(row.startTime);
      const endMin = this.getMinutes(row.endTime);
      const breakInMin = this.getMinutes(row.breakInTime);
      const breakOutMin = this.getMinutes(row.breakOutTime);
      const minStart = this.getMinutes(`${String(startHour).padStart(2, '0')}:00`);
      // 出勤・退勤のバリデーション
      if (startMin !== -1 && startMin < minStart) {
        eventErrors.start = `${startHour}:00以降にしてください。`;
      }
      if (startMin !== -1 && endMin !== -1 && startMin >= endMin) {
        eventErrors.end = '退勤は出勤より後にしてください。';
      }
      if (row.startTime && startMin === -1) {
        eventErrors.start = '正しい時刻を入力してください。';
      }
      if (row.endTime && endMin === -1) {
        eventErrors.end = '正しい時刻を入力してください。';
      }
      // 休憩入り・戻りのバリデーション
      if (row.breakInTime && breakInMin === -1) {
        eventErrors.breakIn = '正しい時刻を入力してください。';
      }
      if (row.breakOutTime && breakOutMin === -1) {
        eventErrors.breakOut = '正しい時刻を入力してください。';
      }
      // 休憩入り・戻りの論理チェック
      if (breakInMin !== -1 && breakOutMin !== -1) {
        if (breakOutMin <= breakInMin) {
          eventErrors.breakOut = '休憩戻り時間は休憩入り時間より後にしてください。';
        }
      }
      // 休憩入りが出勤～退勤の間か
      if (breakInMin !== -1 && startMin !== -1 && endMin !== -1) {
        if (breakInMin < startMin || breakInMin > endMin) {
          eventErrors.breakIn = '休憩入り時間は出勤～退勤の間にしてください。';
        }
      }
      // 休憩戻りが出勤～退勤の間か
      if (breakOutMin !== -1 && startMin !== -1 && endMin !== -1) {
        if (breakOutMin < startMin || breakOutMin > endMin) {
          eventErrors.breakOut = '休憩戻り時間は出勤～退勤の間にしてください。';
        }
      }
      return eventErrors;
    },
    validateAll() {
      this.errors = this.editRows.map(row => this.validateRow(row));
      // どれか1つでもエラーがあればfalse
      return this.errors.every(err => !err.start && !err.end && !err.breakIn && !err.breakOut);
    },
    startEdit() {
      this.isEditing = true;
      this.editRows = this.tableData.map(row => ({ ...row }));
      this.validateAll();
    },
    cancelEdit() {
      const isChanged = this.editRows.some((row, idx) => {
        const original = this.tableData[idx];
        return row.workTime !== original.workTime ||
               row.nightWorkTime !== original.nightWorkTime ||
               row.startTime !== original.startTime ||
               row.endTime !== original.endTime ||
               row.task !== original.task;
      });
      if (isChanged) {
        const result = confirm('内容が保存されていません。ページを移動しますか？');
        if (!result) return;
      }
      this.isEditing = false;
    },
    timeStrToHourDecimal(timeStr) {
      if (!timeStr) return '';
      const [h, m, s] = timeStr.split(':').map(Number);
      return (h + (m / 60) + (s / 3600)).toFixed(1);
    },
    changeMonth(amount) {
      const newDate = new Date(this.calendarDate);
      newDate.setMonth(newDate.getMonth() + amount, 1);
      this.calendarDate = newDate;
    },
    selectDate(day) {
      if (!day.date) return;
      // 選択した日付でEditAttendanceに遷移
      const yyyy = day.date.getFullYear();
      const mm = String(day.date.getMonth() + 1).padStart(2, '0');
      const dd = String(day.date.getDate()).padStart(2, '0');
      this.$router.push({ name: 'Edit-Attendance', params: { date: `${yyyy}-${mm}-${dd}` } });
    },
    isSelected(day) {
      if (!day.date) return false;
      const current = this.dayShiftDate ? new Date(this.dayShiftDate.split('T')[0]) : new Date();
      return day.date.getFullYear() === current.getFullYear() &&
             day.date.getMonth() === current.getMonth() &&
             day.date.getDate() === current.getDate();
    },
  },
  watch: {
    editRows: {
      handler() {
        if (this.isEditing) this.validateAll();
      },
      deep: true
    },
    '$route.params.date': {
      immediate: true,
      handler(newVal, oldVal) {
        if (newVal !== oldVal) {
          this.fetchAttendance();
        }
      }
    },
    '$route.query.date': {
      immediate: true,
      handler(newVal, oldVal) {
        if (newVal !== oldVal) {
          this.fetchAttendance();
        }
      }
    },
  },

  //ヘッダー表示
  components: {
    CommonHeader
  },

  computed: {
    formattedDayShiftDate() {
      if (!this.dayShiftDate) return '';
      // 例: 2025-06-27 or 2025-06-27T00:00:00
      const dateStr = this.dayShiftDate.split('T')[0];
      const [y, m, d] = dateStr.split('-');
      return `${y}年${Number(m)}月${Number(d)}日`;
    },
    formattedMonth() {
      if (!this.dayShiftDate) return '';
      const dateStr = this.dayShiftDate.split('T')[0];
      const [y, m] = dateStr.split('-');
      return `${Number(m)}月`;
    },
    recordAttendanceMonthQuery() {
      if (!this.dayShiftDate) return '';
      const dateStr = this.dayShiftDate.split('T')[0];
      const [y, m] = dateStr.split('-');
      return `${y}-${m.padStart(2, '0')}-01`;
    },
    calendarYear() {
      return this.calendarDate.getFullYear();
    },
    calendarMonth() {
      return this.calendarDate.getMonth() + 1;
    },
    weekDays() {
      return ['日', '月', '火', '水', '木', '金', '土'];
    },
    calendarGrid() {
      const year = this.calendarYear;
      const month = this.calendarMonth - 1;
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
    },
  },
};
</script>

<style scoped>
.edit-attendance {
  padding: 20px;
}
.edit-controls {
  margin-bottom: 10px;
}
table {
  width: 100%;
  border-collapse: collapse;
}
th, td {
  border: 1px solid #ccc;
  padding: 8px;
  text-align: center;
}
th {
  background-color: #f2f2f2;
}
button {
  margin: 0 2px;
  padding: 4px 10px;
}
input[type="text"], input[type="time"] {
  padding: 2px 4px;
  font-size: 14px;
}
.error-message {
  color: red;
  font-size: 10px;
  text-align: left;
}
.slash-cell {
  background: repeating-linear-gradient(135deg, #ccc, #ccc 2px, transparent 2px, transparent 8px);
}
.fetch-error-message {
  color: red;
  margin: 20px 0;
}
/* カレンダー用CSS（CheckAttendance.vueと同じ） */
.calendar-container {
  margin-bottom: 1rem;
  border: 1px solid #ccc;
  padding: 5px;
  border-radius: 5px;
  max-width: 300px;
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
</style>
