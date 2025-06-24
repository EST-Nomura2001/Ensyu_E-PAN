<template>
  <div class="edit-attendance">
    <h1>勤怠編集</h1>
    <p>6月の勤怠一覧に戻る</p>
    <p>6月24日</p>
    <div v-if="fetchError" class="fetch-error-message">データを取得できませんでした</div>
    <div>
      <div class="edit-controls">
        <button v-if="!isEditing" @click="startEdit">編集</button>
        <button v-else @click="saveEdit">保存</button>
        <button v-if="isEditing" @click="cancelEdit">キャンセル</button>
      </div>
      <table>
        <thead>
          <tr>
            <th>名前</th>
            <th>時給</th>
            <th>人件費</th>
            <th>勤務時間</th>
            <th>うち深夜勤務時間</th> <!--要相談-->
            <th>出勤時間</th>
            <th>退勤時間</th>
            <th>休憩入り時間</th>
            <th>休憩戻り時間</th>
            <th>担当業務</th>
          </tr>
        </thead>
        <tbody>
          <tr class="total-row">
            <td>合計</td>
            <td class="slash-cell"></td>
            <td>{{ totalLaborCost }}</td>
            <td>{{ totalWorkTime }}</td>
            <td class="slash-cell"></td>
            <td class="slash-cell"></td>
            <td class="slash-cell"></td>
            <td class="slash-cell"></td>
            <td class="slash-cell"></td>
            <td class="slash-cell"></td>
          </tr>
          <tr v-for="(row, idx) in tableData" :key="idx">
            <td>{{ row.name }}</td>
            <td>{{ row.wage }}</td>
            <td>{{ row.laborCost }}</td>
            <td>{{ row.workTime }}</td>
            <td>{{ row.nightWorkTime }}</td>
            <td v-if="isEditing">
              <input type="time" v-model="editRows[idx].startTime" style="width: 90px;" />
              <div v-if="errors[idx] && errors[idx].start" class="error-message">{{ errors[idx].start }}</div>
            </td>
            <td v-else>{{ row.startTime }}</td>
            <td v-if="isEditing">
              <input type="time" v-model="editRows[idx].endTime" style="width: 90px;" />
              <div v-if="errors[idx] && errors[idx].end" class="error-message">{{ errors[idx].end }}</div>
            </td>
            <td v-else>{{ row.endTime }}</td>
            <td v-if="isEditing">
              <input type="time" v-model="editRows[idx].breakInTime" style="width: 90px;" />
              <div v-if="errors[idx] && errors[idx].breakIn" class="error-message">{{ errors[idx].breakIn }}</div>
            </td>
            <td v-else>{{ row.breakInTime }}</td>
            <td v-if="isEditing">
              <input type="time" v-model="editRows[idx].breakOutTime" style="width: 90px;" />
              <div v-if="errors[idx] && errors[idx].breakOut" class="error-message">{{ errors[idx].breakOut }}</div>
            </td>
            <td v-else>{{ row.breakOutTime }}</td>
            <td v-if="isEditing">
              <input type="text" v-model="editRows[idx].task" style="width: 80px;" />
            </td>
            <td v-else>{{ row.task }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script>
import { getAttendanceByDateStore, updateAttendanceByDateStore } from '../services/api';

export default {
  name: 'EditAttendance',
  props: {
    date: { type: String, required: true }, // 'YYYY-MM-DD'
    storeId: { type: Number, required: true }
  },
  data() {
    return {
      tableData: [],
      isEditing: false,
      editRows: [],
      errors: [],
      totalLaborCost: '',
      totalWorkTime: '',
      totalNightWorkTime: '',
      storeName: '',
      fetchError: false,
    };
  },
  async mounted() {
    await this.fetchAttendance();
  },
  methods: {
    async fetchAttendance() {
      try {
        const res = await getAttendanceByDateStore(this.date, this.storeId);
        this.tableData = res.users || [];
        this.totalLaborCost = res.totalLaborCost || '';
        this.totalWorkTime = res.totalWorkTime || '';
        this.totalNightWorkTime = res.totalNightWorkTime || '';
        this.storeName = res.storeName || '';
        this.fetchError = false;
      } catch (e) {
        this.fetchError = true;
        // ↓↓↓ デモデータ（後で削除してください）↓↓↓
        this.tableData = [
          {
            name: '田中 太郎', wage: 1200, laborCost: 9600, workTime: '8:00', nightWorkTime: '2:00', startTime: '09:00', endTime: '17:00', task: 'レジ', breakInTime: '12:00', breakOutTime: '13:00',
          },
          {
            name: '鈴木 花子', wage: 1100, laborCost: 8800, workTime: '8:00', nightWorkTime: '0:00', startTime: '10:00', endTime: '18:00', task: '品出し', breakInTime: '12:00', breakOutTime: '13:00',
          },
          {
            name: '佐藤 次郎', wage: 1300, laborCost: 10400, workTime: '8:00', nightWorkTime: '1:00', startTime: '13:00', endTime: '21:00', task: '清掃', breakInTime: '12:00', breakOutTime: '13:00',
          },
        ];
        this.totalLaborCost = 28800;
        this.totalWorkTime = '24:00';
        this.totalNightWorkTime = '3:00';
        this.storeName = 'デモ店舗';
        // ↑↑↑ デモデータここまで ↑↑↑
      }
    },
    async saveEdit() {
      if (!this.validateAll()) {
        alert('入力内容に誤りがあります。修正してください。');
        return;
      }
      try {
        await updateAttendanceByDateStore(this.date, this.storeId, this.editRows);
        await this.fetchAttendance();
        this.isEditing = false;
        alert('保存しました');
      } catch (e) {
        alert('保存に失敗しました');
      }
    },
    getMinutes(timeStr) {
      if (!timeStr || typeof timeStr !== 'string' || !timeStr.includes(':')) return -1;
      const [h, m] = timeStr.split(":").map(Number);
      if (isNaN(h) || isNaN(m)) return -1;
      return h * 60 + m;
    },
    validateRow(row) {
      const startHour = 9;
      const eventErrors = { start: null, end: null, breakIn: null, breakOut: null };
      const startMin = this.getMinutes(row.startTime);
      const endMin = this.getMinutes(row.endTime);
      const breakInMin = this.getMinutes(row.breakInTime);
      const breakOutMin = this.getMinutes(row.breakOutTime);
      const minStart = this.getMinutes(`${String(startHour).padStart(2, '0')}:00`);
      // 出勤・退勤のバリデーション
      if (startMin !== -1 && startMin < minStart) {
        eventErrors.start = `${startHour}:00以降にしてください。`;
      }
      if (startMin !== -1 && endMin !== -1 && startMin >= endMin) {
        eventErrors.end = '退勤は出勤より後にしてください。';
      }
      if (row.startTime && startMin === -1) {
        eventErrors.start = '正しい時刻を入力してください。';
      }
      if (row.endTime && endMin === -1) {
        eventErrors.end = '正しい時刻を入力してください。';
      }
      // 休憩入り・戻りのバリデーション
      if (row.breakInTime && breakInMin === -1) {
        eventErrors.breakIn = '正しい時刻を入力してください。';
      }
      if (row.breakOutTime && breakOutMin === -1) {
        eventErrors.breakOut = '正しい時刻を入力してください。';
      }
      // 休憩入り・戻りの論理チェック
      if (breakInMin !== -1 && breakOutMin !== -1) {
        if (breakOutMin <= breakInMin) {
          eventErrors.breakOut = '休憩戻り時間は休憩入り時間より後にしてください。';
        }
      }
      // 休憩入りが出勤～退勤の間か
      if (breakInMin !== -1 && startMin !== -1 && endMin !== -1) {
        if (breakInMin < startMin || breakInMin > endMin) {
          eventErrors.breakIn = '休憩入り時間は出勤～退勤の間にしてください。';
        }
      }
      // 休憩戻りが出勤～退勤の間か
      if (breakOutMin !== -1 && startMin !== -1 && endMin !== -1) {
        if (breakOutMin < startMin || breakOutMin > endMin) {
          eventErrors.breakOut = '休憩戻り時間は出勤～退勤の間にしてください。';
        }
      }
      return eventErrors;
    },
    validateAll() {
      this.errors = this.editRows.map(row => this.validateRow(row));
      // どれか1つでもエラーがあればfalse
      return this.errors.every(err => !err.start && !err.end && !err.breakIn && !err.breakOut);
    },
    startEdit() {
      this.isEditing = true;
      this.editRows = this.tableData.map(row => ({ ...row }));
      this.validateAll();
    },
    cancelEdit() {
      const isChanged = this.editRows.some((row, idx) => {
        const original = this.tableData[idx];
        return row.workTime !== original.workTime ||
               row.nightWorkTime !== original.nightWorkTime ||
               row.startTime !== original.startTime ||
               row.endTime !== original.endTime ||
               row.task !== original.task;
      });
      if (isChanged) {
        const result = confirm('内容が保存されていません。ページを移動しますか？');
        if (!result) return;
      }
      this.isEditing = false;
    }
  },
  watch: {
    editRows: {
      handler() {
        if (this.isEditing) this.validateAll();
      },
      deep: true
    }
  },
};
</script>

<style scoped>
.edit-attendance {
  padding: 20px;
}
.edit-controls {
  margin-bottom: 10px;
}
table {
  width: 100%;
  border-collapse: collapse;
}
th, td {
  border: 1px solid #ccc;
  padding: 8px;
  text-align: center;
}
th {
  background-color: #f2f2f2;
}
button {
  margin: 0 2px;
  padding: 4px 10px;
}
input[type="text"], input[type="time"] {
  padding: 2px 4px;
  font-size: 14px;
}
.error-message {
  color: red;
  font-size: 10px;
  text-align: left;
}
.slash-cell {
  background: repeating-linear-gradient(135deg, #ccc, #ccc 2px, transparent 2px, transparent 8px);
}
.fetch-error-message {
  color: red;
  margin: 20px 0;
}
</style>
