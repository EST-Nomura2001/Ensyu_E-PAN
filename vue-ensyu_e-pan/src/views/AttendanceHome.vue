<template>
  <div class="attendance-home">
    <div class="header">
      <h1>〇〇店</h1>
      <button>新規作成</button>
    </div>
    <table>
      <thead>
        <tr>
          <th>月</th>
          <th>希望収集</th>
          <th>シフト提出期限</th>
          <th>編集画面</th>
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
            <button @click="toggleRecruiting(shift)">【切り替え】</button>
          </td>
          <td>
            <div v-if="shift.isEditingDeadline">
              <input type="date" v-model="shift.editableDeadline" />
              <button @click="saveDeadline(shift)">保存</button>
              <button @click="cancelEditDeadline(shift)">キャンセル</button>
            </div>
            <div v-else>
              {{ formatDeadline(shift.fixedDate) }}
              <button @click="editDeadline(shift)">【設定】</button>
            </div>
          </td>
          <td><button>【編集】</button></td>
          <td>{{ shift.confirmFlg ? '済' : '' }}</td>
          <td><button>【確認】</button></td>
          <td>{{ shift.sendingFlg ? '済' : '' }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
import { getAttendanceData, updateAttendanceData } from '@/services/api';

export default {
  data() {
    return {
      shifts: [], // APIから取得したデータを格納
    };
  },
  methods: {
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
    }
  },
  created() {
    this.fetchShifts();
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