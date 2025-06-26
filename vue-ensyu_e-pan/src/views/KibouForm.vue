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
            </tr>
          </thead>
          <tbody>
            <tr v-for="day in shifts" :key="day.date">
              <td>{{ day.date }}</td>
              <td>{{ getDayOfWeek(day.date) }}</td>
              <td>
                <input type="time" v-model="day.startTime" />
              </td>
              <td>
                <input type="time" v-model="day.endTime" />
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
  },
  methods: {
    async fetchInitialData() {
      this.isLoading = true;
      try {
        // 1. ユーザーIDをsessionStorageから取得
        const userId = Number(sessionStorage.getItem('userId'));
        this.currentUserId = userId;

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
        const shiftArray = schedules && Array.isArray(schedules.$values) ? schedules.$values : [];
        this.shifts = shiftArray.map(s => ({
          date: new Date(s.today).getDate(),
          startTime: s.u_Start_WorkTime ? s.u_Start_WorkTime.substring(11, 16) : '',
          endTime: s.u_End_WorkTime ? s.u_End_WorkTime.substring(11, 16) : ''
        }));

        // 5. その他の情報（必要に応じて）
        // this.userName = ...;
        // this.deadline = ...;
        // this.status = ...;
      } catch (error) {
        console.error('データの取得に失敗しました。', error);
        alert('データの取得に失敗しました。APIサーバーが起動しているか確認してください。');
      } finally {
        this.isLoading = false;
      }
    },
    initializeShifts(existingShifts = []) {
      this.shifts = [];
      const daysInMonth = new Date(this.year, this.month, 0).getDate();
      for (let i = 1; i <= daysInMonth; i++) {
        const existing = existingShifts.find(s => s.date === i);
        this.shifts.push({
          date: i,
          startTime: existing ? existing.startTime : '',
          endTime: existing ? existing.endTime : '',
        });
      }
    },
    getDayOfWeek(date) {
      if (!this.year || !this.month) return '';
      const day = new Date(this.year, this.month - 1, date).getDay();
      const days = ['日', '月', '火', '水', '木', '金', '土'];
      return `(${days[day]})`;
    },
    async submitShifts() {
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
        this.status = '提出済み';
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
  padding: 10px 20px;
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