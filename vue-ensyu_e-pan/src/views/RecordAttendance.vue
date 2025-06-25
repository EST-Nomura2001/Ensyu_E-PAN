<!-- ユーザー各列のGETのみ実装。
    合計列合計行は未実装。 -->

<template>
  <CommonHeader />
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
          <th v-for="ds in dayShifts" :key="'header-'+ds.date">
            <router-link :to="{ name: 'Edit-Attendance', query: { date: ds.date, storeName: storeName } }">
              {{ formatDate(ds.date) }}
            </router-link>
          </th>
        </tr>
      </thead>
      <tbody>
        <tr class="total-row">
          <td class="sticky sticky-1" colspan="2" rowspan="2">合計</td>
          <td class="sticky sticky-3">労働時間(h)</td>
          <td class="sticky sticky-4">{{ totalWorkTime }}</td>
          <td v-for="ds in dayShifts" :key="'total-worktime'+ds.date">{{ ds.sumWorkTime }}</td>
        </tr>
        <tr class="total-row">
          <td class="sticky sticky-3">人件費</td>
          <td class="sticky sticky-4">{{ totalCost }}</td>
          <td v-for="ds in dayShifts" :key="'total-cost'+ds.date">{{ ds.sumTotalCost }}</td>
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
import { fetchAllShiftSchedules, fetchAllShiftsSummary } from '../services/api';

//ヘッダー用
import CommonHeader from '../components/CommonHeader.vue';

export default {
  name: "RecordAttendance",
  data() {
    return {
      status: '',      // ステータス
      storeName: '',   // 店舗名
      yearMonth: '',   // 月
      days: [],        // 日付配列（DB値で上書きするため初期値は空でOK）
      users: [],       // ユーザー配列
      totalWorkTime: 0, // 合計労働時間
      totalCost: 0,     // 合計人件費
      dayShifts: [],    // 日別合計（DBから取得）
      // ↓↓↓ デモ用静的データ（API未接続時のサンプル）
      staticDemoData: {
        status: false, // ALL_SHIFTS.SENDING_FLG
        storeName: '〇店', // STORES.C_NAME
        yearMonth: '2025年6月', // ALL_SHIFTS.DATE
        dayShifts: Array.from({ length: 31 }, (_, i) => ({
          date: `2025-06-${(i+1).toString().padStart(2, '0')}`,
          sumWorkTime: i % 2 === 0 ? 8.0 : 0,
          sumTotalCost: i % 2 === 0 ? 7600 : 0,
        })),
        days: [], // 使わない
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
    // 年月の初期値（例：2025年6月）
    let year = 2025;
    let month = 6;
    // 各人の日付ごとの勤怠データ
    let dateSchedules = [];
    // 合計行・合計列用データ
    let summary = {};
    try {
      // 各人の日付ごとの勤怠データを取得
      dateSchedules = await fetchAllShiftSchedules(year, month);
      // 合計行・合計列用データを取得
      summary = await fetchAllShiftsSummary(year, month);
    } catch (e) {
      // API取得失敗時はデモデータを利用
      dateSchedules = this.staticDemoData.users.flatMap(u => u.days);
      summary = {
        status: this.staticDemoData.status,
        storeName: this.staticDemoData.storeName,
        yearMonth: this.staticDemoData.yearMonth,
        dayShifts: this.staticDemoData.dayShifts,
        totalWorkTime: this.staticDemoData.totalWorkTime,
        totalCost: this.staticDemoData.totalCost,
      };
    }
    // 取得したdateSchedulesをusers配列に整形してセット
    // ここではAPIの返却構造に応じて整形処理が必要です
    // 例：dateSchedulesをユーザーごとにグループ化し、users配列を作成
    this.users = this.formatUsersFromDateSchedules(dateSchedules);
    // 合計行・合計列はsummaryからセット
    this.status = summary.status;
    this.storeName = summary.storeName;
    this.yearMonth = summary.yearMonth;
    this.dayShifts = summary.dayShifts || [];
    this.days = this.dayShifts.map(ds => ds.date);
    this.totalWorkTime = summary.totalWorkTime;
    this.totalCost = summary.totalCost;
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
      // API用のyear, monthをセット
      let year = y;
      let month = m;
      // 各人の日付ごとの勤怠データ
      let dateSchedules = [];
      // 合計行・合計列用データ
      let summary = {};
      try {
        dateSchedules = await fetchAllShiftSchedules(year, month);
        summary = await fetchAllShiftsSummary(year, month);
      } catch (e) {
        dateSchedules = this.staticDemoData.users.flatMap(u => u.days);
        summary = {
          status: this.staticDemoData.status,
          storeName: this.staticDemoData.storeName,
          yearMonth: this.staticDemoData.yearMonth,
          dayShifts: this.staticDemoData.dayShifts,
          totalWorkTime: this.staticDemoData.totalWorkTime,
          totalCost: this.staticDemoData.totalCost,
        };
      }
      // 取得したdateSchedulesをusers配列に整形してセット
      this.users = this.formatUsersFromDateSchedules(dateSchedules);
      // 合計行・合計列はsummaryからセット
      this.status = summary.status;
      this.storeName = summary.storeName;
      this.yearMonth = summary.yearMonth;
      this.dayShifts = summary.dayShifts || [];
      this.days = this.dayShifts.map(ds => ds.date);
      this.totalWorkTime = summary.totalWorkTime;
      this.totalCost = summary.totalCost;
    },
    /**
     * @description APIから取得したdateSchedules配列を、テーブル描画用のusers配列に整形する
     * @param {Array} dateSchedules - APIから取得した日別勤怠データ
     * @returns {Array} users配列
     */
    formatUsersFromDateSchedules(dateSchedules) {
      // ここでdateSchedulesをユーザーごとにグループ化し、
      // [{ name, wage, totalWorkTime, monthPrice, days: [{ workTime, dayPrice }, ...] }] の形に整形する
      // ※APIの返却構造に応じて要調整
      // サンプル実装（仮）
      const userMap = {};
      dateSchedules.forEach(ds => {
        const userName = ds.user?.name || ds.userName || '不明';
        if (!userMap[userName]) {
          userMap[userName] = {
            name: userName,
            wage: ds.user?.wage || ds.wage || '',
            totalWorkTime: 0,
            monthPrice: 0,
            days: []
          };
        }
        userMap[userName].days.push({
          workTime: ds.workTime || '',
          dayPrice: ds.dayPrice || ''
        });
        // 合計値の加算（必要に応じて）
        userMap[userName].totalWorkTime += Number(ds.workTime || 0);
        userMap[userName].monthPrice += Number(ds.dayPrice || 0);
      });
      // 合計値を文字列化
      Object.values(userMap).forEach(u => {
        u.totalWorkTime = u.totalWorkTime.toFixed(1);
        u.monthPrice = u.monthPrice.toString();
      });
      return Object.values(userMap);
    },
    onExport() {
      // 今は空
    },
    formatDate(dateStr) {
      // 例: "2025-06-01" → "6/1"
      const d = new Date(dateStr);
      if (isNaN(d)) return dateStr; // パース失敗時はそのまま
      return `${d.getMonth() + 1}/${d.getDate()}`;
    }
  },

  //ヘッダー用
  components: {
    CommonHeader
  },
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
