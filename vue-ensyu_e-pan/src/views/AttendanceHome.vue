//田村担当
<template>
  <CommonHeader />
  <div class="attendance-home">
    <div class="header">
      <h1>{{ storeName }}</h1>
      <button @click="handleGenerateMonthly">翌月の新規作成</button>
    </div>
    <table>
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
  </div>
</template>

<script>
import { getAttendanceData, updateAttendanceData, getUserInfo, getStoreInfo, generateMonthly } from '@/services/api';
import CommonHeader from '../components/CommonHeader.vue';

export default {
  components: {
    CommonHeader
  },
  data() {
    return {
      shifts: [], // APIから取得したデータを格納
      storeName: '〇〇店', // 初期値
      // ↓↓↓ デモデータ（API連携できない場合用、後で削除）
      demoShifts: [
        {
          id: 1,
          date: '2024-07-01',
          recFlg: true,
          fixedDate: '2024-07-10T00:00:00',
          confirmFlg: true,
          sendingFlg: false,
          isEditingDeadline: false,
          editableDeadline: '2024-07-10',
        },
        {
          id: 2,
          date: '2024-08-01',
          recFlg: false,
          fixedDate: '2024-08-12T00:00:00',
          confirmFlg: false,
          sendingFlg: true,
          isEditingDeadline: false,
          editableDeadline: '2024-08-12',
        },
      ],
      // ↑↑↑ デモデータここまで
    };
  },
  methods: {
    async fetchInitialData() {
      await this.fetchShifts();
      await this.fetchStoreName();
    },
    async fetchStoreName() {
      try {
        // TODO: 実際のログインユーザーIDを使用するように変更
        const userId = 1; 
        const userInfoResponse = await getUserInfo(userId);
        const storeId = userInfoResponse.data.storesCd; // STORES_CD に対応
        if (storeId) {
          const storeInfoResponse = await getStoreInfo(storeId);
          this.storeName = storeInfoResponse.data.c_name;
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
        this.fetchShifts();
      } catch (error) {
        alert('翌月の新規作成に失敗しました。'|| response.data);
        console.error(error);
      }
    },
  },
  created() {
    this.fetchInitialData();
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