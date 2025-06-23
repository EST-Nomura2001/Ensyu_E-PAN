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
    // バックエンド側で、USERSテーブルから該当ユーザーの情報を取得します。
    // GET /api/users/{userId}
    return apiClient.get(`/users/${userId}`);
}; 