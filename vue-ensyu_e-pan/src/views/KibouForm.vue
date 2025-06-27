<!-- ユーザーID指定のみ、未テスト。ユーザー名と提出期限が未実装。
 田村 -->

<template>
  <CommonHeader />
  <div style="margin: 10px 0; padding: 10px; background: #f9f9f9; border: 1px solid #ccc;">
    <label>テスト用ユーザーID: <input type="number" v-model.number="testUserId" style="width: 60px;" /></label>
    <button @click="setTestUserId">設定</button>
    <span style="margin-left: 10px;">現在のuserId: {{ currentUserId }}</span>
  </div>
  <div class="shift-submission-form">
    <h1>希望シフト提出</h1>
    <div v-if="!isLoading && year && month">
      <h2>{{ year }}年{{ month }}月</h2>
      <div>
        <p>{{ userName }}さん</p>
        <p>ステータス：<span>{{ status }}</span></p>
        <p>提出期限：<span>{{ deadline }}</span></p>
      </div>
      <form @submit.prevent="submitShifts">
        <table>
          <thead>
            <tr>
              <th>日付</th>
              <th>曜日</th>
              <th>出勤時間</th>
              <th>退勤時間</th>
              <th>取消</th>
              <th style="width: 260px; min-width: 180px; max-width: 320px; white-space: normal; word-break: break-all;">注意事項</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(day, idx) in shifts" :key="day.date">
              <td>{{ day.date }}</td>
              <td>{{ getDayOfWeek(day.date) }}</td>
              <td>
                <input type="time" v-model="day.startTime" @input="validateShift(idx)" />
              </td>
              <td>
                <input type="time" v-model="day.endTime" @input="validateShift(idx)" />
              </td>
              <td>
                <button type="button" @click="resetShift(day)">取消</button>
              </td>
              <td style="width: 260px; min-width: 180px; max-width: 320px; white-space: normal; word-break: break-all;">
                <span v-if="day.error && (day.error.start || day.error.end)" style="color: red; font-size: 12px;">
                  <span v-if="day.error.start">{{ day.error.start }}</span>
                  <span v-else-if="day.error.end">{{ day.error.end }}</span>
                </span>
              </td>
            </tr>
          </tbody>
        </table>
        <button type="submit">確定</button>
      </form>
    </div>
    <div v-else>
      <p>データを読み込んでいます...</p>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import * as api from '@/services/api';

//ヘッダー用
import CommonHeader from '../components/CommonHeader.vue';

export default {
  props: {
    // ログイン機能実装後、外部からユーザーIDを受け取る
    userId: {
      type: Number,
      default: 1, // 開発用の仮ユーザーID
    },
  },
  data() {
    return {
      isLoading: true,
      year: null,
      month: null,
      deadline: '',
      userName: '',
      status: '未提出', // APIから取得する想定
      shifts: [],
      currentUserId: null,
      testUserId: '', // テスト用userId入力欄
    };
  },
  async created() {
    await this.fetchInitialData();
    // sessionStorageの内容をコンソールに出力
    console.log('userId:', sessionStorage.getItem('userId'));
    console.log('userName:', sessionStorage.getItem('userName'));
    console.log('isAdmin:', sessionStorage.getItem('isAdmin'));
    console.log('storeId:', sessionStorage.getItem('storeId'));
  },
  methods: {
    async fetchInitialData() {
      this.isLoading = true;
      try {
        // 1. ユーザーIDをsessionStorageから取得
        const userId = Number(sessionStorage.getItem('userId'));
        this.currentUserId = userId;
        this.userName = sessionStorage.getItem('userName') || '';

        // 2. 年月は「来月」
        const today = new Date();
        let nextMonth = today.getMonth() + 2; // 1月=0なので+2
        let year = today.getFullYear();
        if (nextMonth > 12) {
          year += 1;
          nextMonth = 1;
        }
        const month = nextMonth;
        this.year = year;
        this.month = month;

        // 3. API呼び出し
        const url = `http://localhost:5011/api/Attendance/UserSchedule/Month/${userId}/${year}/${month}`;
        const response = await axios.get(url);
        const schedules = response.data;
        const shiftArray = Array.isArray(schedules) ? schedules : [];
        this.shifts = shiftArray.map(s => ({
          date: new Date(s.today).getDate(),
          startTime: s.u_Start_WorkTime
            ? (typeof s.u_Start_WorkTime === 'string'
                ? s.u_Start_WorkTime.substring(11, 16)
                : new Date(s.u_Start_WorkTime).toLocaleTimeString('ja-JP', { hour: '2-digit', minute: '2-digit', hour12: false }))
            : '',
          endTime: s.u_End_WorkTime
            ? (typeof s.u_End_WorkTime === 'string'
                ? s.u_End_WorkTime.substring(11, 16)
                : new Date(s.u_End_WorkTime).toLocaleTimeString('ja-JP', { hour: '2-digit', minute: '2-digit', hour12: false }))
            : '',
          error: { start: '', end: '' },
        }));

        // U_Confirm_Flgの取得とstatusへの反映
        // 1件でもtrueがあれば「シフト提出済み」、全てfalseまたはnullなら「シフト未提出」
        const hasConfirmed = shiftArray.some(s => s.u_Confirm_Flg === true);
        this.status = hasConfirmed ? 'シフト提出済み' : 'シフト未提出';
        // ...他の情報（userName, deadline等）は必要に応じて
      } catch (error) {
        console.error('データの取得に失敗しました。', error);
        alert('データの取得に失敗しました。APIサーバーが起動しているか確認してください。');
      } finally {
        this.isLoading = false;
      }
    },
    getDayOfWeek(date) {
      if (!this.year || !this.month) return '';
      const day = new Date(this.year, this.month - 1, date).getDay();
      const days = ['日', '月', '火', '水', '木', '金', '土'];
      return `(${days[day]})`;
    },
    resetShift(day) {
      day.startTime = '';
      day.endTime = '';
    },
    validateShift(idx) {
      const shift = this.shifts[idx];
      shift.error = { start: '', end: '' };
      // 両方未入力はOK
      if (!shift.startTime && !shift.endTime) return;
      // 片方だけ入力はNG（start側だけにエラー文セット）
      if (!shift.startTime || !shift.endTime) {
        shift.error.start = '出勤・退勤時間は両方入力、または両方未入力にしてください。';
        shift.error.end = '';
        return;
      }
      // 出勤時間9:00未満はNG
      if (shift.startTime < '09:00') {
        shift.error.start = '出勤時間は9:00以降にしてください。';
      }
      // 退勤時間24:00超はNG
      if (shift.endTime > '24:00') {
        shift.error.end = '退勤時間は24:00までにしてください。';
      }
      // 退勤時間が出勤時間以前はNG
      if (shift.endTime && shift.startTime && shift.endTime <= shift.startTime) {
        shift.error.end = '退勤時間は出勤時間より後にしてください。';
      }
    },
    validateShifts() {
      for (const shift of this.shifts) {
        // 両方未入力はOK
        if (!shift.startTime && !shift.endTime) continue;
        // 片方だけ入力はNG
        if (!shift.startTime || !shift.endTime) {
          return '出勤・退勤時間は両方入力、または両方未入力にしてください。';
        }
        if (shift.startTime < '09:00') {
          return '出勤時間は9:00以降にしてください。';
        }
        if (shift.endTime > '24:00') {
          return '退勤時間は24:00までにしてください。';
        }
        if (shift.endTime <= shift.startTime) {
          return '退勤時間は出勤時間より後にしてください。';
        }
      }
      return null;
    },
    async submitShifts() {
      const error = this.validateShifts();
      if (error) {
        alert(error);
        return;
      }
      try {
        const userId = this.currentUserId;
        const year = this.year;
        const month = this.month;
        const U_Confirm_Flg = true;

        for (const shift of this.shifts) {
          const yyyy = year;
          const mm = String(month).padStart(2, '0');
          const dd = String(shift.date).padStart(2, '0');
          const dateStr = `${yyyy}-${mm}-${dd}`;
          const targetDate = `${dateStr}T00:00:00`;
          const uStart = shift.startTime ? `${dateStr}T${shift.startTime}:00` : null;
          const uEnd = shift.endTime ? `${dateStr}T${shift.endTime}:00` : null;

          await axios.put(
            `http://localhost:5011/api/Attendance/user/${userId}/schedule-update`,
            {
              TargetDate: targetDate,
              U_Start_WorkTime: uStart,
              U_End_WorkTime: uEnd,
              U_Confirm_Flg: U_Confirm_Flg
            }
          );
        }

        alert('シフトが提出されました。');
        // 提出後は「シフト提出済み」に更新
        this.status = 'シフト提出済み';
      } catch (error) {
        console.error('シフトの提出に失敗しました。', error);
        alert('シフトの提出に失敗しました。');
      }
    },
    setTestUserId() {
      if (this.testUserId) {
        sessionStorage.setItem('userId', this.testUserId);
        this.fetchInitialData();
      } else {
        alert('ユーザーIDを入力してください');
      }
    },
  },
  //ヘッダー用
  components: {
    CommonHeader
  },
};
</script>

<style scoped>
.shift-submission-form {
  max-width: 800px;
  margin: 0 auto;
  padding: 20px;
  font-family: sans-serif;
}

h1 {
  text-align: center;
  margin-bottom: 20px;
}

table {
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 20px;
}

th, td {
  border: 1px solid #ddd;
  padding: 8px;
  text-align: center;
}

th {
  background-color: #f2f2f2;
}

input[type="time"] {
  width: 100%;
  padding: 5px;
  box-sizing: border-box;
  border: 1px solid #ccc;
  border-radius: 4px;
}

button {
  display: block;
  width: 100%;
  background-color: #4CAF50;
  color: white;
  border: none;
  cursor: pointer;
  font-size: 16px;
  border-radius: 4px;
}

button:hover {
  background-color: #45a049;
}
</style> 