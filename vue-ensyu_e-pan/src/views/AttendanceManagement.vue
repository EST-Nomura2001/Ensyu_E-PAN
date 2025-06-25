<template>
  <div class="attendance-management">
    <h1>勤怠管理</h1>
    <table>
      <thead>
        <tr>
          <th>名前</th>
          <th>予定出勤</th>
          <th>予定退勤</th>
          <th>担当業務</th>
          <th>出勤時刻</th>
          <th>退勤時刻</th>
          <th>休憩開始</th>
          <th>休憩終了</th>
          <th v-if="canOperate">操作</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in attendanceData" :key="item.userId">
          <td>{{ item.userName }}</td>
          <td>{{ item.p_start_worktime }}</td>
          <td>{{ item.p_end_worktime }}</td>
          <td>{{ item.taskName }}</td>
          <td>{{ item.start_worktime }}</td>
          <td>{{ item.end_worktime }}</td>
          <td>{{ item.start_break_time }}</td>
          <td>{{ item.end_break_time }}</td>
          <td v-if="canOperate">
            <button @click="clockIn(item.userId)">出勤</button>
            <button @click="clockOut(item.userId)">退勤</button>
            <button @click="startBreak(item.userId)">休憩入</button>
            <button @click="endBreak(item.userId)">休憩戻</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
import { ref, onMounted, computed } from 'vue';
import axios from 'axios';

export default {
  name: 'AttendanceManagement',
  setup() {
    const userRole = ref(sessionStorage.getItem('userRole') || 'partTime');
    const loggedInUserId = ref(parseInt(sessionStorage.getItem('userId')) || 1);
    const allAttendanceData = ref([]);

    const attendanceData = computed(() => {
      if (userRole.value === 'partTime') {
        return allAttendanceData.value.filter(item => item.userId === loggedInUserId.value);
      }
      return allAttendanceData.value;
    });

    const canOperate = computed(() => {
      return userRole.value === 'admin' || userRole.value === 'employee';
    });
    
    const apiClient = axios.create({ baseURL: 'https://localhost:5011/api' });

    const fetchData = async () => {
      try {
        const response = await apiClient.get('/attendance/today');
        allAttendanceData.value = response.data;
      } catch (error) {
        console.error('Failed to fetch attendance data:', error);
        // エラー発生時は空のままにするか、何らかのエラー表示を検討
      }
    };
    
    const clockIn = (userId) => console.log(`Clock in for user ${userId}`);
    const clockOut = (userId) => console.log(`Clock out for user ${userId}`);
    const startBreak = (userId) => console.log(`Start break for user ${userId}`);
    const endBreak = (userId) => console.log(`End break for user ${userId}`);

    onMounted(() => {
      fetchData();
    });

    return {
      attendanceData,
      canOperate,
      clockIn,
      clockOut,
      startBreak,
      endBreak,
    };
  },
};
</script>

<style scoped>
.attendance-management {
  padding: 20px;
}
table {
  width: 100%;
  border-collapse: collapse;
}
th, td {
  border: 1px solid #ddd;
  padding: 8px;
  text-align: center;
}
th {
  background-color: #f2f2f2;
}
button {
  padding: 5px 10px;
  cursor: pointer;
  margin: 0 2px;
}
</style> 