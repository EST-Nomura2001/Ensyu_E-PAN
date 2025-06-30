<template>
  <CommonHeader v-if="canOperate" />
  <PartTimeHeader v-else />
  <div class="attendance-management">
    <h1>勤怠管理</h1>
    <div>
      <label>表示中の日付：</label>
      <span>{{ formatDateJp(selectedDate) }}</span>
    </div>
    <div style="margin-bottom: 16px;">
      <button @click="goToEditAttendance">編集画面へ</button>
    </div>

    <!-- 追加: データがない場合のメッセージ表示 -->
    <div v-if="isNoWorkToday" style="margin-bottom: 16px; color: red;">
      本日の業務はありません
    </div>

    <!-- データがある場合のみテーブルを表示 -->
    <table v-else>
      <thead>
        <tr>
          <th>名前</th>
          <th>担当業務</th>
          <th>予定出勤</th>
          <th>予定退勤</th>
          <th>実出勤</th>
          <th>実退勤</th>
          <th>休憩開始</th>
          <th>休憩終了</th>
          <th v-if="canOperate">操作</th>
        </tr>
      </thead>
      <tbody>
        <template v-for="item in attendanceData" :key="item.id">
          <tr v-for="shift in item.userDateShifts" :key="shift.id">
            <td>{{ item.userName }}</td>
            <td>{{ shift.dateSchedule.workRollName }}</td>
            <td>{{ formatDateTime(shift.dateSchedule.p_Start_WorkTime, 'time') }}</td>
            <td>{{ formatDateTime(shift.dateSchedule.p_End_WorkTime, 'time') }}</td>
            <td>{{ formatDateTime(shift.dateSchedule.start_WorkTime, 'time') }}</td>
            <td>{{ formatDateTime(shift.dateSchedule.end_WorkTime, 'time') }}</td>
            <td>{{ formatDateTime(shift.dateSchedule.start_BreakTime, 'time') }}</td>
            <td>{{ formatDateTime(shift.dateSchedule.end_BreakTime, 'time') }}</td>
            <td v-if="canOperate">
              <button @click="clockIn(item.user_Id, shift.dateSchedule.id)">出勤</button>
              <button @click="clockOut(item.user_Id, shift.dateSchedule.id)">退勤</button>
              <button @click="startBreak(item.user_Id, shift.dateSchedule.id)">休憩入</button>
              <button @click="endBreak(item.user_Id, shift.dateSchedule.id)">休憩戻</button>
            </td>
          </tr>
        </template>
      </tbody>
    </table>
  </div>
</template>

<script>
import { ref, onMounted, computed } from 'vue';
import { fetchDateSchedules, operateAttendance } from '../services/api';
import { useRouter } from 'vue-router';

//ヘッダー用
import CommonHeader from '../components/CommonHeader.vue';
import PartTimeHeader from '../components/PartTimeHeader.vue';

export default {
  name: 'AttendanceManagement',
  setup() {
    const userRole = ref(sessionStorage.getItem('userRole') || 'partTime');
    const loggedInUserId = ref(parseInt(sessionStorage.getItem('userId')) || 1);
    const allAttendanceData = ref([]);
    const todayStr = new Date().toISOString().slice(0, 10);
    const selectedDate = ref(todayStr);
    const router = useRouter();

    const attendanceData = computed(() => {
      if (userRole.value === 'partTime') {
        return allAttendanceData.value.filter(item => item.user_Id === loggedInUserId.value);
      }
      return allAttendanceData.value;
    });

    const canOperate = computed(() => {
      return sessionStorage.getItem('isAdmin') === 'true';
    });

    // 追加: 本日の業務がないか判定
    const isNoWorkToday = computed(() => {
      if (attendanceData.value.length === 0) return true;
      return attendanceData.value.every(item =>
        item.userDateShifts.every(shift =>
          !shift.dateSchedule.p_Start_WorkTime
        )
      );
    });

    const fetchData = async () => {
      try {
        const response = await fetchDateSchedules(selectedDate.value);
        // データ整形処理を追加
        const formatted = response.data.flatMap(day =>
          day.dateSchedules.map(ds => ({
            user_Id: ds.userId,
            userName: ds.userName,
            userDateShifts: [
              {
                id: ds.id,
                dateSchedule: ds
              }
            ]
          }))
        );
        allAttendanceData.value = formatted;
        console.log('整形後:', formatted);
      } catch (error) {
        console.error('Failed to fetch attendance data:', error);
      }
    };

    // 日付・時刻フォーマット用
    const formatDateTime = (value, type = 'datetime') => {
      if (!value) return '';
      const date = new Date(value);
      if (type === 'time') {
        return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
      }
      return date.toLocaleString();
    };

    // 日付を「2025年6月27日」形式で返す関数
    const formatDateJp = (dateStr) => {
      if (!dateStr) return '';
      const date = new Date(dateStr);
      const year = date.getFullYear();
      const month = date.getMonth() + 1;
      const day = date.getDate();
      return `${year}年${month}月${day}日`;
    };

    // JSTのISO文字列を返す関数
    const toJstISOString = (date) => {
      const jst = new Date(date.getTime() + (9 * 60 * 60 * 1000));
      return jst.toISOString().replace('Z', '+09:00');
    };

    // 勤怠操作ボタン用
    const clockIn = async (userId, scheduleId) => {
      try {
        // 対象のシフト情報を取得
        const item = allAttendanceData.value.find(i => i.user_Id === userId);
        const shift = item?.userDateShifts.find(s => s.id === scheduleId);
        const ds = shift?.dateSchedule;
        if (!ds) throw new Error('シフト情報が見つかりません');
        // UpdateDateScheduleDtoを作成
        const now = new Date();
        const updateDto = {
          Id: ds.id,
          User_Id: userId,
          Work_Roll_Id: ds.workRollId,
          Day_Shift_Id: ds.dayShiftId,
          Start_WorkTime: toJstISOString(now),
          End_WorkTime: ds.end_WorkTime,
          Start_BreakTime: ds.start_BreakTime,
          End_BreakTime: ds.end_BreakTime
        };
        await operateAttendance(userId, scheduleId, updateDto);
        await fetchData();
      } catch (e) {
        alert('出勤処理に失敗しました');
      }
    };
    const clockOut = async (userId, scheduleId) => {
      try {
        const item = allAttendanceData.value.find(i => i.user_Id === userId);
        const shift = item?.userDateShifts.find(s => s.id === scheduleId);
        const ds = shift?.dateSchedule;
        if (!ds) throw new Error('シフト情報が見つかりません');
        const now = new Date();
        const updateDto = {
          Id: ds.id,
          User_Id: userId,
          Work_Roll_Id: ds.workRollId,
          Day_Shift_Id: ds.dayShiftId,
          Start_WorkTime: ds.start_WorkTime,
          End_WorkTime: toJstISOString(now),
          Start_BreakTime: ds.start_BreakTime,
          End_BreakTime: ds.end_BreakTime
        };
        await operateAttendance(userId, scheduleId, updateDto);
        await fetchData();
      } catch (e) {
        alert('退勤処理に失敗しました');
      }
    };
    const startBreak = async (userId, scheduleId) => {
      try {
        const item = allAttendanceData.value.find(i => i.user_Id === userId);
        const shift = item?.userDateShifts.find(s => s.id === scheduleId);
        const ds = shift?.dateSchedule;
        if (!ds) throw new Error('シフト情報が見つかりません');
        const now = new Date();
        const updateDto = {
          Id: ds.id,
          User_Id: userId,
          Work_Roll_Id: ds.workRollId,
          Day_Shift_Id: ds.dayShiftId,
          Start_WorkTime: ds.start_WorkTime,
          End_WorkTime: ds.end_WorkTime,
          Start_BreakTime: toJstISOString(now),
          End_BreakTime: ds.end_BreakTime
        };
        await operateAttendance(userId, scheduleId, updateDto);
        await fetchData();
      } catch (e) {
        alert('休憩入処理に失敗しました');
      }
    };
    const endBreak = async (userId, scheduleId) => {
      try {
        const item = allAttendanceData.value.find(i => i.user_Id === userId);
        const shift = item?.userDateShifts.find(s => s.id === scheduleId);
        const ds = shift?.dateSchedule;
        if (!ds) throw new Error('シフト情報が見つかりません');
        const now = new Date();
        const updateDto = {
          Id: ds.id,
          User_Id: userId,
          Work_Roll_Id: ds.workRollId,
          Day_Shift_Id: ds.dayShiftId,
          Start_WorkTime: ds.start_WorkTime,
          End_WorkTime: ds.end_WorkTime,
          Start_BreakTime: ds.start_BreakTime,
          End_BreakTime: toJstISOString(now)
        };
        await operateAttendance(userId, scheduleId, updateDto);
        await fetchData();
      } catch (e) {
        alert('休憩戻処理に失敗しました');
      }
    };

    // 編集画面へ遷移
    const goToEditAttendance = () => {
      router.push({ name: 'Edit-Attendance' });
    };

    onMounted(() => {
      fetchData();
      // sessionStorageの内容をコンソールに出力
      console.log('userId:', sessionStorage.getItem('userId'));
      console.log('userName:', sessionStorage.getItem('userName'));
      console.log('isAdmin:', sessionStorage.getItem('isAdmin'));
      console.log('storeId:', sessionStorage.getItem('storeId'));
    });

    return {
      attendanceData,
      canOperate,
      clockIn,
      clockOut,
      startBreak,
      endBreak,
      selectedDate,
      fetchData,
      formatDateTime,
      formatDateJp,
      goToEditAttendance,
      isNoWorkToday
    };
  },

  //ヘッダー用
  components: {
    CommonHeader,
    PartTimeHeader
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