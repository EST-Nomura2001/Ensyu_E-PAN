<template>
  <div>
    <h1>勤怠実績確認</h1>
    <div class="attendance-table-controls" style="margin-bottom: 10px; display: flex; gap: 10px; align-items: center;">
      <button @click="changeMonth(-1)">前の月へ</button>
      <h2>{{ yearMonth }}</h2>
      <button @click="changeMonth(1)">次の月へ</button>
    </div>
    <p>{{ storeName }}</p>
    <p>ステータス：{{ status === true ? '送付済み' : '未提出' }}</p>
    <p>日付をクリックすると、詳細の確認・修正が可能です。</p>
  </div>
  <div class="attendance-table-wrapper">
    <table class="attendance-table">
      <thead>
        <tr>
          <th class="sticky sticky-1">名前</th>
          <th class="sticky sticky-2">時給</th>
          <th class="sticky sticky-3">項目</th>
          <th class="sticky sticky-4">合計</th>
          <th v-for="day in days" :key="day">{{ day }}日</th>
        </tr>
      </thead>
      <tbody>
        <tr class="total-row">
          <td class="sticky sticky-1" colspan="2" rowspan="2">合計</td>
          <td class="sticky sticky-3">労働時間(h)</td>
          <td class="sticky sticky-4">{{ totalWorkTime }}</td>
          <td v-for="day in days" :key="'total-worktime'+day"></td>
        </tr>
        <tr class="total-row">
          <td class="sticky sticky-3">人件費</td>
          <td class="sticky sticky-4">{{ totalCost }}</td>
          <td v-for="day in days" :key="'total-cost'+day"></td>
        </tr>
        <template v-for="user in users" :key="user.name">
          <tr>
            <td class="sticky sticky-1" :rowspan="2">{{ user.name }}</td>
            <td class="sticky sticky-2" :rowspan="2">{{ user.wage }}</td>
            <td class="sticky sticky-3">労働時間(h)</td>
            <td class="sticky sticky-4">{{ user.totalWorkTime }}</td>
            <td v-for="(d, idx) in user.days" :key="'worktime'+user.name+idx">{{ d.workTime }}</td>
          </tr>
          <tr>
            <td class="sticky sticky-3">人件費</td>
            <td class="sticky sticky-4">{{ user.monthPrice }}</td>
            <td v-for="(d, idx) in user.days" :key="'cost'+user.name+idx">{{ d.dayPrice }}</td>
          </tr>
        </template>
      </tbody>
    </table>
  </div>
  <div class="table-footer">
    <button class="export-btn" @click="onExport">出力</button>
  </div>
</template>

<script>
import { fetchAttendanceData } from '../services/api';

export default {
  name: "RecordAttendance",
  data() {
    return {
      status: '',      // ステータス
      storeName: '',   // 店舗名
      yearMonth: '',   // 月
      days: [],        // 日付配列
      users: [],       // ユーザー配列
      totalWorkTime: 0, // 合計労働時間
      totalCost: 0,     // 合計人件費
      // ↓↓↓ デモ用静的データ（API未接続時のサンプル）
      staticDemoData: {
        status: false, // ALL_SHIFTS.SENDING_FLG
        storeName: '〇店', // STORES.C_NAME
        yearMonth: '2025年6月', // ALL_SHIFTS.DATE
        days: Array.from({ length: 31 }, (_, i) => i + 1),
        totalWorkTime: '123.5', // ALL_SHIFTS.SUM_WORKTIME
        totalCost: '456789', // ALL_SHIFTS.COST
        users: [
          {
            name: '名前1', // USERS.NAME
            wage: 950,    // USERS.TIMEPRICE_D
            totalWorkTime: '40.0', // USER_SHIFTS.TOTAL_WORKTIME
            monthPrice: '38000',   // USER_SHIFTS.MONTH_PRICE
            days: Array.from({ length: 31 }, (_, i) => ({
              workTime: i % 2 === 0 ? '8.0' : '', // DATE_SCHEDULES.T_WORKTIME_ALL
              dayPrice: i % 2 === 0 ? '7600' : '', // DATE_SCHEDULES.T_DAY_PRICE
            })),
          },
          {
            name: '名前2', wage: 1000, totalWorkTime: '30.0', monthPrice: '30000',
            days: Array.from({ length: 31 }, (_, i) => ({
              workTime: i % 3 === 0 ? '6.0' : '',
              dayPrice: i % 3 === 0 ? '6000' : '',
            })),
          },
          {
            name: '名前3', wage: 850, totalWorkTime: '20.0', monthPrice: '17000',
            days: Array.from({ length: 31 }, (_, i) => ({
              workTime: i % 5 === 0 ? '4.0' : '',
              dayPrice: i % 5 === 0 ? '3400' : '',
            })),
          },
        ],
      },
      // ↑↑↑ デモ用静的データここまで
    };
  },
  async mounted() {
    // storeId, yearMonthは適宜指定
    const storeId = 1;
    const yearMonth = '2025-06';
    let data;
    try {
      data = await fetchAttendanceData({ storeId, yearMonth });
    } catch (e) {
      // ↓↓↓ API取得失敗時はデモデータを利用
      data = this.staticDemoData;
    }
    // 例：APIレスポンスの構造に合わせてセット
    this.status = data.status; // ALL_SHIFTS.SENDING_FLG
    this.storeName = data.storeName; // STORES.C_NAME
    this.yearMonth = data.yearMonth; // ALL_SHIFTS.DATE
    this.days = data.days; // [1,2,...,31]
    this.users = data.users; // [{ name, wage, totalWorkTime, monthPrice, days: [{ workTime, dayPrice }, ...] }]
    this.totalWorkTime = data.totalWorkTime; // ALL_SHIFTS.SUM_WORKTIME
    this.totalCost = data.totalCost; // ALL_SHIFTS.COST
  },
  methods: {
    async changeMonth(diff) {
      // yearMonth: 'YYYY年M月' or 'YYYY-MM' どちらでも対応
      let y, m;
      if (this.yearMonth.includes('年')) {
        // 表示用（例: 2025年6月）
        const match = this.yearMonth.match(/(\d{4})年(\d{1,2})月/);
        y = parseInt(match[1]);
        m = parseInt(match[2]);
      } else {
        // API用（例: 2025-06）
        const match = this.yearMonth.match(/(\d{4})-(\d{1,2})/);
        y = parseInt(match[1]);
        m = parseInt(match[2]);
      }
      m += diff;
      if (m < 1) { y--; m = 12; }
      if (m > 12) { y++; m = 1; }
      const newYearMonth = `${y}-${m.toString().padStart(2, '0')}`;
      // storeIdは仮で1固定（必要に応じてstoreIdをdata等で管理）
      const storeId = 1;
      let data;
      try {
        data = await fetchAttendanceData({ storeId, yearMonth: newYearMonth });
      } catch (e) {
        data = this.staticDemoData;
      }
      this.status = data.status;
      this.storeName = data.storeName;
      this.yearMonth = data.yearMonth;
      this.days = data.days;
      this.users = data.users;
      this.totalWorkTime = data.totalWorkTime;
      this.totalCost = data.totalCost;
    },
    onExport() {
      // 今は空
    }
  }
};
</script>

<style scoped>
.attendance-table-wrapper {
  overflow-x: auto;
  width: 100%;
}
.attendance-table {
  border-collapse: collapse;
  width: 100%;
  min-width: 900px;
  font-size: 14px;
}
.attendance-table th,
.attendance-table td {
  border: 1px solid #333;
  padding: 4px 8px;
  text-align: center;
  background: #fff;
  min-width: 90px;
  white-space: nowrap;
}
.attendance-table th {
  background: #f0f0f0;
}
.total-row,
.total-row td,
.total-row th {
  background: #e0f7fa !important;
  font-weight: bold;
}
.attendance-table th.sticky,
.attendance-table td.sticky {
  position: sticky;
  background: #fff;
  background-clip: padding-box;
  z-index: 10;
}
.attendance-table th.sticky-1,
.attendance-table td.sticky-1 {
  left: 0;
  box-shadow: 1px 0 0 0 #333;
}
.attendance-table th.sticky-2,
.attendance-table td.sticky-2 {
  left: 90px;
  box-shadow: 1px 0 0 0 #333;
}
.attendance-table th.sticky-3,
.attendance-table td.sticky-3 {
  left: 180px;
  box-shadow: 1px 0 0 0 #333;
}
.attendance-table th.sticky-4,
.attendance-table td.sticky-4 {
  left: 270px;
  z-index: 20;
  box-shadow: 2px 0 0 0 #333;
}
.total-row .sticky-1,
.total-row .sticky-2 {
  box-shadow: none !important;
}
.total-row .sticky {
  background: #e0f7fa !important;
}
</style>
