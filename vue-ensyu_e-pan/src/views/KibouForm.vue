<template>
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
import * as api from '@/services/api';

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
    };
  },
  async created() {
    // DEMO: 静的デモデータを使用（後で削除してください）
    this.isLoading = false;
    this.year = 2025;
    this.month = 6;
    this.deadline = '5月20日';
    this.userName = '山田太郎';
    this.status = '未提出';
    // 6月分のデモシフトデータ
    this.shifts = [
      { date: 1, startTime: '09:00', endTime: '18:00' },
      { date: 2, startTime: '', endTime: '' },
      { date: 3, startTime: '10:00', endTime: '17:00' },
      { date: 4, startTime: '', endTime: '' },
      { date: 5, startTime: '', endTime: '' },
      { date: 6, startTime: '09:00', endTime: '18:00' },
      { date: 7, startTime: '', endTime: '' },
      // ... 必要に応じて追加 ...
    ];
    // 本来はAPIから取得する
    // await this.fetchInitialData();
  },
  methods: {
    async fetchInitialData() {
      this.isLoading = true;
      try {
        // 1. 募集中のシフト情報を取得
        const recruitingInfo = await api.getRecruitingShiftInfo();
        // APIのレスポンスが { data: { year: 2025, month: 6, deadline: '5月20日' } } のような形式を想定
        this.year = recruitingInfo.data.year;
        this.month = recruitingInfo.data.month;
        this.deadline = recruitingInfo.data.deadline;

        // 2. ユーザー情報を取得
        const userInfo = await api.getUserInfo(this.userId);
        // APIのレスポンスが { data: { name: '山田太郎' } } のような形式を想定
        this.userName = userInfo.data.name;

        // 3. 既存の希望シフトを取得
        const userShifts = await api.getUserShifts(this.userId, this.year, this.month);
        // APIのレスポンスが { data: [{ date: 1, startTime: '09:00', endTime: '18:00' }, ...] } を想定
        const existingShifts = userShifts.data;

        // 4. シフト表を初期化
        this.initializeShifts(existingShifts);

      } catch (error) {
        console.error('データの取得に失敗しました。', error);
        alert('データの取得に失敗しました。APIサーバーが起動しているか確認してください。');
        // 必要に応じてエラー表示用のUIをここに実装
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
        await api.submitUserShifts(this.userId, this.shifts);
        alert('シフトが提出されました。');
        this.status = '提出済み'; // 画面上のステータスを更新
      } catch (error) {
        console.error('シフトの提出に失敗しました。', error);
        alert('シフトの提出に失敗しました。');
      }
    },
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