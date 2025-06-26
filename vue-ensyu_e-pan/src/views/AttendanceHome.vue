<!--田村担当-->
<!-- 
## 1. fetchShiftsByYear
- 使用API: `getAllShiftsForAllMonths`
- 内容: 指定した年の全月分のシフトデータを取得します。

## 2. fetchStoreName
- 使用API: `getStoreInfo`
- 内容: sessionStorageからstoreIdを取得し、店舗名をAPIで取得します。

## 3. toggleRecruiting
- 使用API: `updateAttendanceData`
- 内容: 希望収集フラグ（recFlg）を切り替えてAPIで更新します。

## 4. saveDeadline
- 使用API: `updateAttendanceData`
- 内容: シフト提出期限（fixedDate）をAPIで更新します。

## 5. handleGenerateMonthly
- 使用API: `generateMonthly`
- 内容: 翌月のシフトデータを新規作成するAPIを呼び出します。
 -->
<template>
  <CommonHeader />
  <div class="attendance-home">
    <div class="header">
      <div style="display: flex; align-items: center; gap: 8px;">
        <label>店舗ID:</label>
        <input type="text" v-model="storeId" style="width: 80px;" />
        <button @click="setStoreId">店舗ID設定</button>
      </div>
      <div style="display: flex; align-items: center; gap: 8px;">
        <input
          type="number"
          v-model.number="selectedYear"
          min="2000"
          max="2100"
          @input="validateYear"
          style="width: 90px;"
        />
        <button @click="fetchShiftsByYear">指定年のシフト取得</button>
        <button @click="handleGenerateMonthly">翌月の新規作成</button>
      </div>
    </div>
    <div v-if="noDataMessage" style="color: red; margin-bottom: 10px;">{{ noDataMessage }}</div>
    <table v-if="shifts.length > 0">
      <thead>
        <tr>
          <th>月</th>
          <th>希望収集</th>
          <th>シフト提出期限</th>
          <th>シフト表</th>
          <th>シフト編集状態</th>
          <th>勤怠画面</th>
          <th>勤怠送付</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="shift in shifts" :key="shift.id">
          <td>{{ formatYearMonth(shift.date) }}</td>
          <td>
            {{ shift.recFlg ? '集計中' : '非収集中' }}
            <button @click="toggleRecruiting(shift)">切り替え</button>
          </td>
          <td>
            <div v-if="shift.isEditingDeadline">
              <input type="date" v-model="shift.editableDeadline" />
              <button @click="saveDeadline(shift)">保存</button>
              <button @click="cancelEditDeadline(shift)">キャンセル</button>
            </div>
            <div v-else>
              {{ formatDeadline(shift.fixedDate) }}
              <button @click="editDeadline(shift)">設定</button>
            </div>
          </td>
          <td>編集 / 閲覧</td>
          <td>{{ shift.confirmFlg ? '済' : '' }}</td>
          <td>閲覧</td>
          <td>{{ shift.sendingFlg ? '済' : '' }}</td>
        </tr>
      </tbody>
    </table>
    <div v-else style="margin-top: 20px; color: #666;">データがありません</div>
  </div>
</template>

<script>
import { getAttendanceData, updateAttendanceData, getUserInfo, getStoreInfo, generateMonthly, getAllShiftsForAllMonths } from '@/services/api';
import CommonHeader from '../components/CommonHeader.vue';

export default {
  components: {
    CommonHeader
  },
  data() {
    return {
      shifts: [], // APIから取得したデータを格納
      selectedYear: new Date().getFullYear(),
      noDataMessage: '',
      storeId: sessionStorage.getItem('storeId') || '', // テスト用inputのバインド用
    };
  },
  methods: {
    validateYear() {
      if (this.selectedYear < 2000) this.selectedYear = 2000;
      if (this.selectedYear > 2100) this.selectedYear = 2100;
    },
    async fetchShiftsByYear() {
      this.noDataMessage = '';
      const storeId = sessionStorage.getItem('storeId');
      if (!storeId) {
        this.noDataMessage = '店舗IDが取得できません';
        return;
      }
      const year = this.selectedYear;
      const monthPromises = [];
      for (let m = 1; m <= 12; m++) {
        monthPromises.push(
          getAllShiftsForAllMonths
            ? getAllShiftsForAllMonths(storeId, year, m)
            : this.$axios.get(`/api/Attendance/store/${storeId}/allshifts/${year}/${m}`)
                .then(res => {
                  const arr = res.data;
                  if (!Array.isArray(arr) || arr.length === 0) return [];
                  return arr;
                })
                .catch(() => [])
        );
      }
      const results = await Promise.all(monthPromises);
      // 12ヶ月分の配列をフラット化
      this.shifts = results.flat();
      console.log('APIレスポンス（各月ごと）:', results);
      console.log('shifts:', this.shifts);
      if (this.shifts.length === 0) {
        this.noDataMessage = '指定した年のデータがありません';
      } else {
        console.log('1件目:', this.shifts[0]);
      }
    },
    async toggleRecruiting(shift) {
      try {
        const response = await updateAttendanceData(shift.id, { recFlg: !shift.recFlg });
        const updatedShift = response.data;
        const index = this.shifts.findIndex(s => s.id === shift.id);
        if (index !== -1) {
          this.shifts[index].recFlg = updatedShift.recFlg;
        }
      } catch (error) {
        console.error('希望収集状態の更新に失敗しました。', error);
      }
    },
    editDeadline(shift) {
      shift.isEditingDeadline = true;
    },
    cancelEditDeadline(shift) {
      shift.isEditingDeadline = false;
      shift.editableDeadline = shift.fixedDate ? shift.fixedDate.split('T')[0] : '';
    },
    async saveDeadline(shift) {
      try {
        const response = await updateAttendanceData(shift.id, { fixedDate: shift.editableDeadline });
        const updatedShift = response.data;
        const index = this.shifts.findIndex(s => s.id === shift.id);
        if (index !== -1) {
          this.shifts[index].fixedDate = updatedShift.fixedDate;
          this.shifts[index].isEditingDeadline = false;
        }
      } catch (error) {
        console.error('提出期限の更新に失敗しました。', error);
      }
    },
    formatYearMonth(dateString) {
      if (!dateString) return '';
      const date = new Date(dateString);
      return `${date.getFullYear()}年${date.getMonth() + 1}月`;
    },
    formatDeadline(dateString) {
      if (!dateString) return '';
      const date = new Date(dateString);
      if (date.getFullYear() === 9999) return '';
      return `${date.getMonth() + 1}月${date.getDate()}日`;
    },
    async handleGenerateMonthly() {
      try {
        // 今日の日付をYYYY-MM-DD形式で取得
        const today = new Date();
        const yyyy = today.getFullYear();
        const mm = String(today.getMonth() + 1).padStart(2, '0');
        const dd = String(today.getDate()).padStart(2, '0');
        const postDate = `${yyyy}-${mm}-${dd}`;

        const response = await generateMonthly(postDate);
        alert(response.data || '翌月のシフトデータを作成しました。');
        this.fetchShiftsByYear();
      } catch (error) {
        alert('翌月の新規作成に失敗しました。'|| (error && error.response && error.response.data));
        console.error(error);
      }
    },
    async setStoreId() {
      if (!this.storeId) {
        alert('店舗IDを入力してください');
        return;
      }
      sessionStorage.setItem('storeId', this.storeId);
      await this.fetchShiftsByYear();
    },
  },
  created() {
    // storeIdがあれば初期化
    if (sessionStorage.getItem('storeId')) {
      this.fetchShiftsByYear();
    }
  }
};
</script>

<style scoped>
.attendance-home {
  padding: 20px;
}
.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}
table {
  width: 100%;
  border-collapse: collapse;
}
th,
td {
  border: 1px solid #ccc;
  padding: 8px;
  text-align: center;
}
th {
  background-color: #f2f2f2;
}
button {
  margin-left: 5px;
}
</style> 