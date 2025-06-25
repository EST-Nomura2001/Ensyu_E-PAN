<!--田村担当-->

<template>
  <CommonHeader />
  <div class="attendance-home">
    <div class="header">
      <h1>{{ storeName }}</h1>
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
      storeName: '〇〇店', // 初期値
      selectedYear: new Date().getFullYear(),
      noDataMessage: '',
    };
  },
  methods: {
    validateYear() {
      if (this.selectedYear < 2000) this.selectedYear = 2000;
      if (this.selectedYear > 2100) this.selectedYear = 2100;
    },
    async fetchShiftsByYear() {
      this.noDataMessage = '';
      try {
        const allShifts = await getAllShiftsForAllMonths(this.selectedYear);
        if (allShifts.length === 0) {
          this.shifts = [];
          this.noDataMessage = '指定した年のデータがありません';
        } else {
          // ALL_SHIFTSテーブルのカラム名に合わせてマッピング
          this.shifts = allShifts.map(shift => ({
            id: shift.ID || shift.id, // ID
            date: shift.DATE || shift.date, // 月
            recFlg: shift.REC_FLG ?? shift.recFlg, // 希望収集
            fixedDate: shift.FIXED_DATE || shift.fixedDate, // シフト提出期限
            confirmFlg: shift.CONFIRM_FLG ?? shift.confirmFlg, // シフト編集状態
            sendingFlg: shift.SENDING_FLG ?? shift.sendingFlg, // 勤怠送付
            // 既存のUI用プロパティ
            isEditingDeadline: false,
            editableDeadline: (shift.FIXED_DATE || shift.fixedDate) ? (shift.FIXED_DATE || shift.fixedDate).split('T')[0] : '',
          }));
        }
      } catch (error) {
        this.shifts = [];
        this.noDataMessage = 'データ取得に失敗しました';
        console.error('シフト情報の取得に失敗しました。', error);
      }
    },
    async fetchStoreName() {
      try {
        const storeId = sessionStorage.getItem('storeId');
        if (storeId) {
          const storeInfoResponse = await getStoreInfo(storeId);
          this.storeName = storeInfoResponse.data.c_name;
        } else {
          this.storeName = '店舗ID未設定';
        }
      } catch (error) {
        console.error('店舗名の取得に失敗しました。', error);
        this.storeName = '店舗名取得エラー';
      }
    },
    async fetchShifts() {
      try {
        const response = await getAttendanceData();
        this.shifts = response.data.map(shift => ({
          ...shift,
          isEditingDeadline: false,
          editableDeadline: shift.fixedDate ? shift.fixedDate.split('T')[0] : '',
        }));
      } catch (error) {
        console.error('シフト情報の取得に失敗しました。', error);
        // ↓↓↓ デモデータ（API連携できない場合用、後で削除）
        this.shifts = this.demoShifts.map(shift => ({ ...shift }));
        // ↑↑↑ デモデータここまで
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
        const response = await generateMonthly();
        alert(response.data || '翌月のシフトデータを作成しました。');
        this.fetchShiftsByYear();
      } catch (error) {
        alert('翌月の新規作成に失敗しました。'|| response.data);
        console.error(error);
      }
    },
  },
  created() {
    this.fetchShiftsByYear();
    this.fetchStoreName();
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