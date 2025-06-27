<!--  ユーザー各列のGETのみ実装。
      合計列合計行は未実装。 -->

<template>
  <CommonHeader />
  <template v-if="!checkedAuth">
    <div>認証確認中...</div>
  </template>
  <template v-else-if="!isAdmin">
    <div style="color: red; font-size: 1.2em; margin: 2em;">権限がありません</div>
  </template>
  <template v-else>
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
              <router-link :to="{ name: 'Edit-Attendance', params: { date: ds.date } }">
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
      <button class="export-btn" disabled>出力</button>
    </div>
  </template>

<script>
import { getAllShiftsForAllMonths } from '../services/api';

//ヘッダー用
import CommonHeader from '../components/CommonHeader.vue';

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
      dayShifts: [],    // 日別合計
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
    let year, month;
    // クエリパラメータdateがあれば年月を取得
    if (this.$route.query.date) {
      const date = new Date(this.$route.query.date);
      if (!isNaN(date)) {
        year = date.getFullYear();
        month = date.getMonth() + 1;
      }
    }
    // なければデフォルト
    if (!year || !month) {
      const now = new Date();
      year = now.getFullYear();
      month = now.getMonth() + 1;
    }
    const storeId = sessionStorage.getItem('storeId');
    let allShifts = [];
    try {
      allShifts = await getAllShiftsForAllMonths(storeId, year, month);
    } catch (e) {
      console.error('API error:', e);
      allShifts = [];
    }
    if (!Array.isArray(allShifts)) allShifts = [];
    // --- ネスト構造を展開してusers配列・days配列を生成 ---
    const userMap = {};
    const allDatesSet = new Set();
    (allShifts || []).forEach(monthData => {
      (monthData.userShifts || []).forEach(userShift => {
        const userId = userShift.user_Id || userShift.userName;
        if (!userMap[userId]) {
          userMap[userId] = {
            name: userShift.userName,
            wage: '', // 必要ならuserShiftから取得
            totalWorkTime: 0,
            monthPrice: 0,
            days: []
          };
        }
        (userShift.userDateShifts || []).forEach(dateShift => {
          const ds = dateShift.dateSchedule || {};
          allDatesSet.add(ds.today);
          userMap[userId].days.push({
            workTime: ds.t_WorkTime_All ? this.timeStrToHourDecimal(ds.t_WorkTime_All).toFixed(1) : '',
            dayPrice: ds.t_DayPrice || ''
          });
          userMap[userId].totalWorkTime += ds.t_WorkTime_All ? this.timeStrToHourDecimal(ds.t_WorkTime_All) : 0;
          userMap[userId].monthPrice += Number(ds.t_DayPrice || 0);
        });
      });
    });
    // 日付配列を昇順で
    const allDates = Array.from(allDatesSet).filter(Boolean).sort();
    // users配列を整形
    const users = Object.values(userMap).map(u => ({
      ...u,
      totalWorkTime: u.totalWorkTime ? u.totalWorkTime.toFixed(1) : '',
      monthPrice: u.monthPrice ? u.monthPrice.toString() : '',
    }));
    // 日ごとの合計
    const dayShifts = allDates.map((date, idx) => {
      let sumWorkTime = users.reduce((sum, u) => sum + Number(u.days[idx]?.workTime || 0), 0);
      let sumTotalCost = users.reduce((sum, u) => sum + Number(u.days[idx]?.dayPrice || 0), 0);
      return {
        date,
        sumWorkTime: sumWorkTime ? sumWorkTime.toFixed(1) : '',
        sumTotalCost: sumTotalCost ? sumTotalCost.toString() : ''
      };
    });
    // 合計
    const totalWorkTime = users.reduce((sum, u) => sum + Number(u.totalWorkTime || 0), 0).toFixed(1);
    const totalCost = users.reduce((sum, u) => sum + Number(u.monthPrice || 0), 0).toString();
    this.users = users;
    this.days = allDates;
    this.dayShifts = dayShifts;
    this.totalWorkTime = totalWorkTime;
    this.totalCost = totalCost;
    this.status = '';
    this.storeName = '';
    this.yearMonth = `${year}年${month}月`;
  },
  methods: {
    timeStrToHourDecimal(timeStr) {
      if (!timeStr) return 0;
      const [h, m, s] = timeStr.split(':').map(Number);
      return h + (m / 60) + (s / 3600);
    },
    async changeMonth(diff) {
      let y, m;
      if (this.yearMonth.includes('年')) {
        const match = this.yearMonth.match(/(\d{4})年(\d{1,2})月/);
        y = parseInt(match[1]);
        m = parseInt(match[2]);
      } else {
        const match = this.yearMonth.match(/(\d{4})-(\d{1,2})/);
        y = parseInt(match[1]);
        m = parseInt(match[2]);
      }
      m += diff;
      if (m < 1) { y--; m = 12; }
      if (m > 12) { y++; m = 1; }
      let year = y;
      let month = m;
      const storeId = sessionStorage.getItem('storeId');
      let allShifts = [];
      try {
        allShifts = await getAllShiftsForAllMonths(storeId, year, month);
      } catch (e) {
        console.error('API error:', e);
        allShifts = [];
      }
      if (!Array.isArray(allShifts)) allShifts = [];
      // --- ネスト構造を展開してusers配列・days配列を生成 ---
      const userMap = {};
      const allDatesSet = new Set();
      (allShifts || []).forEach(monthData => {
        (monthData.userShifts || []).forEach(userShift => {
          const userId = userShift.user_Id || userShift.userName;
          if (!userMap[userId]) {
            userMap[userId] = {
              name: userShift.userName,
              wage: '', // 必要ならuserShiftから取得
              totalWorkTime: 0,
              monthPrice: 0,
              days: []
            };
          }
          (userShift.userDateShifts || []).forEach(dateShift => {
            const ds = dateShift.dateSchedule || {};
            allDatesSet.add(ds.today);
            userMap[userId].days.push({
              workTime: ds.t_WorkTime_All ? this.timeStrToHourDecimal(ds.t_WorkTime_All).toFixed(1) : '',
              dayPrice: ds.t_DayPrice || ''
            });
            userMap[userId].totalWorkTime += ds.t_WorkTime_All ? this.timeStrToHourDecimal(ds.t_WorkTime_All) : 0;
            userMap[userId].monthPrice += Number(ds.t_DayPrice || 0);
          });
        });
      });
      // 日付配列を昇順で
      const allDates = Array.from(allDatesSet).filter(Boolean).sort();
      // users配列を整形
      const users = Object.values(userMap).map(u => ({
        ...u,
        totalWorkTime: u.totalWorkTime ? u.totalWorkTime.toFixed(1) : '',
        monthPrice: u.monthPrice ? u.monthPrice.toString() : '',
      }));
      // 日ごとの合計
      const dayShifts = allDates.map((date, idx) => {
        let sumWorkTime = users.reduce((sum, u) => sum + Number(u.days[idx]?.workTime || 0), 0);
        let sumTotalCost = users.reduce((sum, u) => sum + Number(u.days[idx]?.dayPrice || 0), 0);
        return {
          date,
          sumWorkTime: sumWorkTime ? sumWorkTime.toFixed(1) : '',
          sumTotalCost: sumTotalCost ? sumTotalCost.toString() : ''
        };
      });
      // 合計
      const totalWorkTime = users.reduce((sum, u) => sum + Number(u.totalWorkTime || 0), 0).toFixed(1);
      const totalCost = users.reduce((sum, u) => sum + Number(u.monthPrice || 0), 0).toString();
      this.users = users;
      this.days = allDates;
      this.dayShifts = dayShifts;
      this.totalWorkTime = totalWorkTime;
      this.totalCost = totalCost;
      this.status = '';
      this.storeName = '';
      this.yearMonth = `${year}年${month}月`;
    },
    formatDate(dateStr) {
      // 例: "2025-06-01" → "6/1"
      const d = new Date(dateStr);
      if (isNaN(d)) return dateStr; // パース失敗時はそのまま
      return `${d.getMonth() + 1}/${d.getDate()}`;
    },
    printTable() {
      window.print();
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

/* 印刷用スタイル */
@media print {
  html, body {
    width: 100% !important;
    height: auto !important;
    margin: 0 !important;
    padding: 0 !important;
    overflow: visible !important;
  }
  .attendance-table-wrapper {
    width: 100vw !important;
    max-width: 100vw !important;
    overflow: visible !important;
  }
  .attendance-table {
    width: 100% !important;
    min-width: unset !important;
    font-size: 9pt !important;
    table-layout: fixed !important;
    word-break: break-all;
  }
  .attendance-table th,
  .attendance-table td {
    min-width: unset !important;
    padding: 1px 2px !important;
    font-size: 9pt !important;
    word-break: break-all;
  }
  /* ページ幅に合わせて縮小 */
  body {
    zoom: 0.7;
  }
}
