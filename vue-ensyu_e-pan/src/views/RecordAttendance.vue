<!--  ユーザー各列のGETのみ実装。
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
    <button class="export-btn" disabled>出力</button>
  </div>
</template>

<script>
import { fetchAllShiftSchedules } from '../services/api';

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
    };
  },
  async mounted() {
    // ★初回表示は2025年7月に固定（本実装時は下記2行を有効化）
    let year = 2025;
    let month = 7;
    // const now = new Date();
    // let year = now.getFullYear();
    // let month = now.getMonth() + 1;
    let dateSchedules = [];
    try {
      dateSchedules = await fetchAllShiftSchedules(year, month);
      if (dateSchedules && dateSchedules.$values) {
        dateSchedules = dateSchedules.$values;
      }
    } catch (e) {
      console.error('API error:', e);
      dateSchedules = [];
    }
    if (!Array.isArray(dateSchedules)) dateSchedules = [];
    // --- 合計列合計行（ALL_SHIFTS.SUM_WORKTIME） ---
    let totalWorkTime = '';
    if (dateSchedules.length > 0) {
      const userShifts = dateSchedules[0]?.user?.userShifts?.$values || [];
      if (userShifts.length > 0 && userShifts[0].allShift) {
        totalWorkTime = userShifts[0].allShift.sum_WorkTime || '';
      }
    }
    // --- 全日付（DATE_SCHEDULES.TODAY）をユニークかつ昇順で抽出 ---
    function toDateString(dateStr) {
      if (!dateStr) return '';
      return new Date(dateStr).toISOString().split('T')[0];
    }
    const allDates = Array.from(
      new Set(dateSchedules.map(ds => toDateString(ds.today)).filter(Boolean))
    ).sort();
    // --- 合計行各日付（DAY_SHIFTS.SUM_WORKTIME） ---
    const dayShiftMap = {};
    dateSchedules.forEach(ds => {
      if (ds.dayShift && ds.dayShift.id) {
        dayShiftMap[ds.dayShift.id] = ds.dayShift;
      }
    });
    const dayShifts = Object.values(dayShiftMap).sort((a, b) => new Date(a.date) - new Date(b.date));
    // --- 合計列個人行（USER_SHIFTS.TOTAL_WORKTIME） ---
    // ユーザーごとの列は一旦空配列にする（別メソッドで取得予定）
    this.users = [];
    // 合計行・合計列はusersから算出
    this.status = '';
    this.storeName = '';
    this.yearMonth = `${year}年${month}月`;
    this.days = allDates;
    this.totalWorkTime = totalWorkTime;
    this.totalCost = this.users.reduce((sum, u) => sum + Number(u.monthPrice || 0), 0).toString();
    this.dayShifts = dayShifts.map(ds => ({
      date: ds.date,
      sumWorkTime: ds.sum_WorkTime || '',
      sumTotalCost: ds.sum_TotalCost || ''
    }));
  },
  methods: {
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
      let dateSchedules = [];
      try {
        dateSchedules = await fetchAllShiftSchedules(year, month);
        if (dateSchedules && dateSchedules.$values) {
          dateSchedules = dateSchedules.$values;
        }
      } catch (e) {
        console.error('API error:', e);
        dateSchedules = [];
      }
      if (!Array.isArray(dateSchedules)) dateSchedules = [];
      if (dateSchedules.length > 0 && dateSchedules[0].days) {
        this.users = dateSchedules.map(u => ({
          name: u.name,
          wage: u.wage,
          totalWorkTime: u.days.reduce((sum, d) => sum + Number(d.workTime || 0), 0).toFixed(1),
          monthPrice: u.days.reduce((sum, d) => sum + Number(d.dayPrice || 0), 0).toString(),
          days: u.days
        }));
      } else {
        this.users = this.formatUsersFromDateSchedules(dateSchedules);
      }
      // 合計行・合計列はusersから算出
      this.status = '';
      this.storeName = '';
      this.yearMonth = `${year}年${month}月`;
      this.days = this.users[0]?.days.map((d, idx) => {
        return `${year}-${month.toString().padStart(2, '0')}-${(idx+1).toString().padStart(2, '0')}`;
      }) || [];
      this.totalWorkTime = this.users.reduce((sum, u) => sum + Number(u.totalWorkTime || 0), 0).toFixed(1);
      this.totalCost = this.users.reduce((sum, u) => sum + Number(u.monthPrice || 0), 0).toString();
      this.dayShifts = this.days.map((date, idx) => {
        let sumWorkTime = this.users.reduce((sum, u) => sum + Number(u.days[idx]?.workTime || 0), 0);
        let sumTotalCost = this.users.reduce((sum, u) => sum + Number(u.days[idx]?.dayPrice || 0), 0);
        return {
          date,
          sumWorkTime: sumWorkTime ? sumWorkTime.toFixed(1) : '',
          sumTotalCost: sumTotalCost ? sumTotalCost.toString() : ''
        };
      });
    },
    /**
     * @description APIから取得したdateSchedules配列を、テーブル描画用のusers配列に整形する
     * @param {Array} dateSchedules - APIから取得した日別勤怠データ
     * @returns {Array} users配列
     */
    formatUsersFromDateSchedules(dateSchedules) {
      // 防御: 配列でなければ空配列を返す
      if (!Array.isArray(dateSchedules)) return [];
      // ユーザーごとにグループ化
      const userMap = {};
      dateSchedules.forEach(ds => {
        // ユーザーIDでグループ化（IDがなければ名前で）
        const userId = ds.userId || ds.user?.id || ds.userName || '不明';
        const userName = ds.user?.name || ds.userName || '不明';
        const wage = ds.user?.timePrice_D || ds.user?.wage || ds.wage || '';
        if (!userMap[userId]) {
          userMap[userId] = {
            name: userName,
            wage: wage,
            totalWorkTime: 0,
            monthPrice: 0,
            days: []
          };
        }
        userMap[userId].days.push({
          workTime: ds.t_WorkTime_All?.toString() || ds.workTime?.toString() || '',
          dayPrice: ds.t_Day_Price?.toString() || ds.dayPrice?.toString() || ''
        });
        userMap[userId].totalWorkTime += Number(ds.t_WorkTime_All || ds.workTime || 0);
        userMap[userId].monthPrice += Number(ds.t_Day_Price || ds.dayPrice || 0);
      });
      // 合計値を文字列化
      Object.values(userMap).forEach(u => {
        u.totalWorkTime = u.totalWorkTime.toFixed(1);
        u.monthPrice = u.monthPrice.toString();
      });
      return Object.values(userMap);
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
