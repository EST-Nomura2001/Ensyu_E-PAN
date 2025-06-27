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
 * 指定店舗・年月のAllShiftデータを取得します。
 * @param {number|string} storeId - 店舗ID
 * @param {number} year - 年
 * @param {number} month - 月
 * @returns {Promise<Array>} - その月のAllShiftDto配列
 */
export async function getAllShiftsForAllMonths(storeId, year, month) {
  const API_BASE_URL = 'http://localhost:5011/api';
  try {
    const response = await axios.get(`${API_BASE_URL}/Attendance/store/${storeId}/allshifts/${year}/${month}`);
    return response.data;
  } catch (error) {
    console.error('店舗・年月指定のAllShiftデータ取得失敗:', error);
    throw error;
  }
}

/**
 * @description ALL_SHIFTSテーブルのrecFlgを更新します。
 * @param {number} id ALL_SHIFTSテーブルのID
 * @param {boolean} recFlg 希望収集フラグ
 * @returns {Promise<void>} 成功時はNoContent
 */
export function updateRecFlag(id, recFlg) {
  // URL直指定、bodyはプリミティブ型で送信
  return axios.put(`http://localhost:5011/api/Attendance/allshift/${id}/rec-flag`, recFlg, {
    headers: { 'Content-Type': 'application/json' }
  });
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

export function fetchDateSchedules(today) {
  // baseURLは使わず、URLを直指定
  return axios.get(`http://localhost:5011/api/Attendance/DateSchedules/${today}`);
}

/**
 * 勤怠操作（出勤・退勤・休憩入・休憩戻）用API
 * @param {number} userId
 * @param {number} scheduleId
 * @param {string} action - 'clockIn' | 'clockOut' | 'startBreak' | 'endBreak'
 * @returns {Promise}
 */
export function operateAttendance(userId, scheduleId, action) {
  // actionごとにエンドポイントやbodyを変えたい場合はここで分岐
  // 今回はURLのみ指定、bodyは空
  return axios.put(`http://localhost:5011/api/Attendance/users/${userId}/schedules/${scheduleId}/${action}`, {});
} 