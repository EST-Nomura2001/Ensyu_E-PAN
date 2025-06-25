//田村担当

import axios from 'axios';

// このURLはASP.NET Web APIのプロジェクトの実行URLに合わせて適宜変更してください。
// 例: https://localhost:7123/api
const apiClient = axios.create({
  baseURL: '/api', // Viteのプロキシ設定を利用することを想定
  headers: {
    'Content-Type': 'application/json',
  },
});

const API_BASE_URL = 'http://localhost:5011/api'; // ASP.NET Web API or Node.js server address

/**
 * @description シフト希望を募集中の年月と提出期限を取得します。
 * @returns {Promise<Object>} { year, month, deadline } を含むPromiseオブジェクト
 */
export const getRecruitingShiftInfo = () => {
  // バックエンド側で、ALL_SHIFTSテーブルからREC_FLGがTRUEのレコードを検索し、
  // 該当する年月と提出期限などを返却することを想定しています。
  // GET /api/shifts/recruiting
  return apiClient.get('/shifts/recruiting');
};

/**
 * @description 特定ユーザーの、指定年月の希望シフト情報を取得します。
 * @param {number} userId ユーザーID
 * @param {number} year 年
 * @param {number} month 月
 * @returns {Promise<Array>} 希望シフトの配列を返すPromiseオブジェクト
 */
export const getUserShifts = (userId, year, month) => {
  // バックエンド側で、DATE_SCHEDULESテーブルなどから該当ユーザーのデータを検索し、
  // 日付、開始時間、終了時間の一覧を返却することを想定しています。
  // GET /api/users/{userId}/shifts?year={year}&month={month}
  return apiClient.get(`/users/${userId}/shifts`, { params: { year, month } });
};

/**
 * @description 希望シフトをサーバーに提出（保存）します。
 * @param {number} userId ユーザーID
 * @param {Array<Object>} shifts 希望シフトのデータ配列
 * @returns {Promise<Object>} 成功レスポンスを返すPromiseオブジェクト
 */
export const submitUserShifts = (userId, shifts) => {
  // バックエンド側で、受け取ったデータをDATE_SCHEDULESテーブルなどに保存します。
  // POST /api/users/{userId}/shifts
  const payload = {
    shifts: shifts.filter(day => day.startTime && day.endTime) // 入力があるものだけ送信
  };
  return apiClient.post(`/users/${userId}/shifts`, payload);
};

/**
 * @description ユーザー情報を取得します。
 * @param {number} userId ユーザーID
 * @returns {Promise<Object>} { name } を含むユーザー情報のPromiseオブジェクト
 */
export const getUserInfo = (userId) => {
    // GET /api/users/{userId}
    return axios.get(`http://localhost:5011/api/users/${userId}`);
};

/**
 * @description 月次シフトの一覧を取得します。
 * @returns {Promise<Array>} 月次シフト情報の配列
 */
export const getAttendanceData = () => {
    return apiClient.get('/AttendanceHome');
};

/**
 * @description 月次シフト情報を更新します。
 * @param {number} id 更新対象のシフトID
 * @param {Object} data 更新するデータ (例: { fixedDate: '...' } or { recFlg: ... })
 * @returns {Promise<Object>} 更新後の月次シフト情報
 */
export const updateAttendanceData = (id, data) => {
    return apiClient.patch(`/AttendanceHome/${id}`, data);
};

/**
 * @description 指定したIDの店舗情報を取得します。
 * @param {number} storeId 店舗ID
 * @returns {Promise<Object>} 店舗情報
 */
export const getStoreInfo = (storeId) => {
    return axios.get(`http://localhost:5011/api/stores/${storeId}`);
};

/**
 * 指定された日付のシフトデータを取得します。
 * @param {Date} date - 取得するシフトの日付
 * @returns {Promise<Object>} - 店舗名とスケジュールリストを含むオブジェクト
 */
export async function getShiftsByDate(date) {
  const dateString = date.toISOString().split('T')[0]; // YYYY-MM-DD
  try {
    const response = await fetch(`${API_BASE_URL}/shifts/${dateString}`);
    if (!response.ok) {
      throw new Error('Network response was not ok');
    }
    return await response.json();
  } catch (error) {
    console.error('Error fetching shift data:', error);
    // In a real app, you'd want to show a user-friendly error
    // For now, returning mock data to allow UI development
    return getMockShiftData(date);
  }
}

/**
 * スケジュールを更新します。
 * @param {number} scheduleId - 更新するスケジュールのID (DATE_SCHEDULES.ID)
 * @param {Object} data - 更新するデータ { workRollName, plannedStart, plannedEnd }
 * @returns {Promise<Object>} - 更新されたスケジュールオブジェクト
 */
export async function updateSchedule(scheduleId, data) {
  try {
    const response = await fetch(`${API_BASE_URL}/schedules/${scheduleId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });
    if (!response.ok) {
      throw new Error('Network response was not ok');
    }
    return await response.json();
  } catch (error) {
    console.error('Error updating schedule:', error);
    throw error;
  }
}

/**
 * 複数のスケジュールを一括で更新します。
 * @param {Array<Object>} schedules - 更新するスケジュールの配列
 * @returns {Promise<Object>} - 更新結果
 */
export async function updateSchedulesBulk(schedules) {
  try {
    const response = await fetch(`${API_BASE_URL}/schedules/bulk`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(schedules),
    });
    if (!response.ok) {
      throw new Error('Network response was not ok');
    }
    return await response.json();
  } catch (error) {
    console.error('Error bulk updating schedules:', error);
    // In a real app, you'd want to show a user-friendly error
    throw error;
  }
}

// --- Mock Data for UI Development ---
// This part should be removed when the backend API is ready.
function getMockShiftData(date) {
    const month = date.getMonth() + 1;
    const day = date.getDate();

    return {
        storeName: `〇〇店`,
        schedules: [
            {
                scheduleId: 101,
                userName: "田中 太郎",
                hourlyWage: 1200,
                workRollName: "レジ",
                hopeStart: "10:00",
                hopeEnd: "14:30",
                plannedStart: "10:00",
                plannedEnd: "11:30"
            },
            {
                scheduleId: 102,
                userName: "鈴木 花子",
                hourlyWage: 1150,
                workRollName: "品出し",
                hopeStart: "12:00",
                hopeEnd: "15:00",
                plannedStart: "13:00",
                plannedEnd: "14:00"
            },
            {
                scheduleId: 103,
                userName: "佐藤 次郎",
                hourlyWage: 1250,
                workRollName: "清掃",
                hopeStart: "14:00",
                hopeEnd: "17:00",
                plannedStart: "15:00",
                plannedEnd: "16:30"
            },
        ]
    }
}

// 勤怠実績データ取得
export async function fetchAttendanceData({ storeId, yearMonth }) {
  // 例: /api/attendance?storeId=1&date=2025-06
  const res = await axios.get('/api/attendance', {
    params: { storeId, date: yearMonth }
  });
  return res.data;
}

// 勤怠実績データ更新（関数名を競合回避のため変更）
export async function updateAttendanceRecord(payload) {
  // payloadは編集内容
  const res = await axios.put('/api/attendance', payload);
  return res.data;
}

// 店舗一覧取得
export async function fetchStores() {
  const res = await axios.get('/api/stores');
  return res.data;
}

// ユーザー一覧取得
export async function fetchUsers({ storeId }) {
  const res = await axios.get('/api/users', { params: { storeId } });
  return res.data;
}

/**
 * 勤怠編集画面用：指定日付・店舗の勤怠データ取得
 * @param {string} date - 'YYYY-MM-DD' 形式
 * @param {number} storeId
 * @returns {Promise<Object>} 勤怠データ
 */
export function getAttendanceByDateStore(date, storeId) {
  return apiClient.get(`/attendance`, { params: { date, storeId } });
}

/**
 * 勤怠編集画面用：指定日付・店舗の勤怠データ保存
 * @param {string} date - 'YYYY-MM-DD' 形式
 * @param {number} storeId
 * @param {Array<Object>} users - 編集後のユーザーデータ配列
 * @returns {Promise<Object>} 保存結果
 */
export function updateAttendanceByDateStore(date, storeId, users) {
  return apiClient.put(`/attendance/${date}/${storeId}`, { users });
}

/**勤怠ホーム画面、新規作成ボタンに対応。
 * @description 翌月のシフトデータを新規作成します。
 * @returns {Promise<Object>} 成功レスポンス
 */
export const generateMonthly = (postDate) => {
  return axios.post(`http://localhost:5011/api/Attendance/generate-monthly/${postDate}`);
};

/**
 * @description 指定年月の全ユーザーの日別勤怠実績（RecordAttendanceの各人の日付列用）を取得します。
 * @param {number} year 年（例: 2025）
 * @param {number} month 月（例: 6）
 * @returns {Promise<Array>} DateScheduleの配列
 */
export async function fetchAllShiftSchedules(year, month) {
  // Swagger: /api/Attendance/AllShiftSchedules/{year}/{month}
  // 例: http://localhost:5011/api/Attendance/AllShiftSchedules/2025/6
  try {
    const response = await axios.get(
      `http://localhost:5011/api/Attendance/AllShiftSchedules/${year}/${month}`
    );
    return response.data;
  } catch (error) {
    console.error('全ユーザー日別勤怠データ取得失敗:', error);
    throw error;
  }
}

/**
 * @description 指定年月の全体合計（労働時間・人件費など）を取得します。
 * @param {number} year 年
 * @param {number} month 月
 * @returns {Promise<Object>} 合計値オブジェクト
 */
export async function fetchAllShiftsSummary(year, month) {
  // 合計行・合計列用API（例: /api/Attendance/AllShiftsSummary/{year}/{month}）
  try {
    const response = await axios.get(
      `http://localhost:5011/api/Attendance/AllShiftsSummary/${year}/${month}`
    );
    return response.data;
  } catch (error) {
    console.error('全体合計データ取得失敗:', error);
    throw error;
  }
}

// 日付指定でDateScheduleデータ取得
export async function getDateSchedulesByDate(today) {
  // todayは "YYYY-MM-DD" 形式で渡すことを推奨
  const response = await axios.get(`http://localhost:5011/api/Attendance/DateSchedules/${today}`);
  return response.data;
}

/**
 * 指定した年の1～12月分のAllShiftSchedulesを一括取得します。
 * @param {number} year - 取得したい年
 * @returns {Promise<Array>} - 12ヶ月分のデータをまとめた配列
 */
export async function getAllShiftsForAllMonths(year) {
  // 1月～12月分をまとめて取得
  const API_BASE_URL = 'http://localhost:5011/api';
  const requests = Array.from({ length: 12 }, (_, i) =>
    axios.get(`${API_BASE_URL}/Attendance/AllShiftSchedules/${year}/${i + 1}`)
      .then(res => res.data)
      .catch(() => []) // 1ヶ月分の取得失敗時は空配列
  );
  const results = await Promise.all(requests);
  return results.flat();
} 