<!-- ユーザーID指定のみ、未テスト。ユーザー名と提出期限が未実装。
 田村 -->

<template>
  <CommonHeader v-if="canOperate" />
  <PartTimeHeader v-else />
  <!--<div style="margin: 10px 0; padding: 10px; background: #f9f9f9; border: 1px solid #ccc;">
    <label>テスト用ユーザーID: <input type="number" v-model.number="testUserId" style="width: 60px;" /></label>
    <button @click="setTestUserId">設定</button>
    <span style="margin-left: 10px;">現在のuserId: {{ currentUserId }}</span>
  </div>-->
  <div class="shift-submission-form">
    <h1>希望シフト提出</h1>
    <div v-if="!isLoading && year && month">
      <h2>{{ year }}年{{ month }}月</h2>
      <div style="margin-bottom: 10px;">
        <label>選択してください：
          <select v-model="year" @change="onYearMonthChange">
            <option v-for="y in yearOptions" :key="y" :value="y">{{ y }}</option>
          </select> 年
        </label>
        <label style="margin-left: 10px;">
          <select v-model="month" @change="onYearMonthChange">
            <option v-for="m in monthOptions" :key="m" :value="m">{{ m }}</option>
          </select> 月
        </label>
      </div>
      
      <div v-if="shifts.length === 0">
        <p style="color: #888; font-size: 18px; text-align: center; margin: 40px 0;">当月のシフトは提出期間対象外です</p>
      </div>
      <div v-else>
        <p>{{ userName }}さん</p>
        <p>ステータス：<span>{{ status }}</span></p>
        <!-- <p>提出期限：<span>{{ deadline }}</span></p> -->
      </div>
      <form v-if="shifts.length > 0" @submit.prevent="submitShifts">
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
import PartTimeHeader from '../components/PartTimeHeader.vue';

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
      yearOptions: [],
      monthOptions: [1,2,3,4,5,6,7,8,9,10,11,12],
      deadline: '',
      userName: '',
      status: '未提出', // APIから取得する想定
      shifts: [],
      currentUserId: Number(sessionStorage.getItem('userId')) || null, // sessionStorageからuserId取得
      testUserId: '', // テスト用userId入力欄
      originalShifts: [], // 追加: 取得直後のshiftsを保持
      isDirty: false,     // 追加: 編集フラグ
    };
  },
  async created() {
    // 年の選択肢（今年と来年）
    const thisYear = new Date().getFullYear();
    this.yearOptions = [thisYear, thisYear + 1];
    // 初期値は来月
    const today = new Date();
    let nextMonth = today.getMonth() + 2; // 1月=0なので+2
    let year = today.getFullYear();
    if (nextMonth > 12) {
      year += 1;
      nextMonth = 1;
    }
    this.year = year;
    this.month = nextMonth;
    await this.fetchInitialData();
    // 追加: 初期データ取得後にoriginalShiftsをセット
    this.originalShifts = JSON.parse(JSON.stringify(this.shifts));
    this.isDirty = false;
  },
  watch: {
    shifts: {
      handler(newVal) {
        this.isDirty = JSON.stringify(newVal) !== JSON.stringify(this.originalShifts);
      },
      deep: true
    }
  },
  beforeRouteLeave(to, from, next) {
    if (this.isDirty) {
      if (window.confirm('内容が保存されていません。ページを移動しますか？')) {
        next();
      } else {
        next(false);
      }
    } else {
      next();
    }
  },
  methods: {
    async fetchInitialData() {
      this.isLoading = true;
      try {
        // 1. ユーザーIDをsessionStorageから取得
        const userId = Number(sessionStorage.getItem('userId'));
        this.currentUserId = userId;
        this.userName = sessionStorage.getItem('userName') || '';

        // 2. 年月は this.year, this.month を使う
        const year = this.year;
        const month = this.month;

        // 3. API呼び出し
        const url = `http://localhost:5011/api/Attendance/UserSchedule/Month/${userId}/${year}/${month}`;
        const response = await axios.get(url);
        const schedules = response.data;
        const shiftArray = Array.isArray(schedules) ? schedules : [];
        this.shifts = shiftArray.map(s => {
          const schedule = (s.dateSchedules && s.dateSchedules[0]) ? s.dateSchedules[0] : {};
          // 日付は s.date または schedule.today どちらでもOK
          let dateValue = null;
          if (s.date) {
            const d = new Date(s.date);
            if (!isNaN(d)) {
              dateValue = d.getDate();
            }
          }
          return {
            date: dateValue,
            startTime: schedule.u_Start_WorkTime
              ? (typeof schedule.u_Start_WorkTime === 'string'
                  ? schedule.u_Start_WorkTime.substring(11, 16)
                  : new Date(schedule.u_Start_WorkTime).toLocaleTimeString('ja-JP', { hour: '2-digit', minute: '2-digit', hour12: false }))
              : '',
            endTime: schedule.u_End_WorkTime
              ? (typeof schedule.u_End_WorkTime === 'string'
                  ? schedule.u_End_WorkTime.substring(11, 16)
                  : new Date(schedule.u_End_WorkTime).toLocaleTimeString('ja-JP', { hour: '2-digit', minute: '2-digit', hour12: false }))
              : '',
            error: { start: '', end: '' },
          };
        });

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
        // 追加: データ取得後にoriginalShiftsを更新
        this.originalShifts = JSON.parse(JSON.stringify(this.shifts));
        this.isDirty = false;
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
        // 追加: 送信後にoriginalShiftsを更新
        this.originalShifts = JSON.parse(JSON.stringify(this.shifts));
        this.isDirty = false;
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
    onYearMonthChange() {
      if (this.isDirty) {
        if (!window.confirm('内容が保存されていません。年月を変更しますか？')) {
          return;
        }
      }
      this.fetchInitialData();
    },
  },
  computed: {
    canOperate() {
      return sessionStorage.getItem('isAdmin') === 'true';
    }
  },
  components: {
    CommonHeader,
    PartTimeHeader
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