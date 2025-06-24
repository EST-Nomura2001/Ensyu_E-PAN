<template>
  <div class="edit-attendance">
    <h1>勤怠編集</h1>
    <table>
      <thead>
        <tr>
          <th>名前</th>
          <th>時給</th>
          <th>人件費</th>
          <th>勤務時間</th>
          <th>深夜勤務時間</th>
          <th>出勤時間</th>
          <th>退勤時間</th>
          <th>担当業務</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(row, idx) in tableData" :key="idx">
          <td>{{ row.name }}</td>
          <td>{{ row.wage }}</td>
          <td>{{ row.laborCost }}</td>
          <td>{{ row.workTime }}</td>
          <td>{{ row.nightWorkTime }}</td>
          <td>{{ row.startTime }}</td>
          <td>{{ row.endTime }}</td>
          <td>{{ row.task }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
export default {
  name: 'EditAttendance',
  data() {
    return {
      tableData: [
        {
          name: '田中 太郎',
          wage: 1200,
          laborCost: 9600,
          workTime: '8:00',
          nightWorkTime: '2:00',
          startTime: '09:00',
          endTime: '17:00',
          task: 'レジ',
        },
        {
          name: '鈴木 花子',
          wage: 1100,
          laborCost: 8800,
          workTime: '8:00',
          nightWorkTime: '0:00',
          startTime: '10:00',
          endTime: '18:00',
          task: '品出し',
        },
        {
          name: '佐藤 次郎',
          wage: 1300,
          laborCost: 10400,
          workTime: '8:00',
          nightWorkTime: '1:00',
          startTime: '13:00',
          endTime: '21:00',
          task: '清掃',
        },
      ],
    };
  },
};
</script>

<style scoped>
.edit-attendance {
  padding: 20px;
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
</style>
