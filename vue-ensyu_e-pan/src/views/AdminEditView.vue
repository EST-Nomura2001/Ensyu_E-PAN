<script>
import axios from 'axios';
import {ref,onMounted} from 'vue';

const users = ref([]);

onMounted(async()=>{
    try {
        const response = await axios.get('http://localhost:5011/api/Account/users/all');
        users.value = response.data;
    } catch (error) {
        console.error('ユーザー取得エラー:', error);
    }
});
</script>
<template>
    <div>
        <h2>アカウント一覧</h2>

        <table>
            <thead>
                <tr>
                <th>ID</th>
                <th>ログインID</th>
                <th>氏名</th>
                <th>ロール</th>
                <th>管理者権限</th>
                <th>所属店舗</th>
                <th>日勤時給</th>
                <th>夜勤時給</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="user in users" :key="user.id">
                <td>{{ user.id }}</td>
                <td>{{ user.login_Id }}</td>
                <td>{{ user.name }}</td>
                <td>{{ user.rollName }}</td>
                <td>{{ user.isAdmin ? 'Admin' : '-' }}</td>
                <td>{{ user.storeName }}</td>
                <td>{{ user.timePrice_D }} 円</td>
                <td>{{ user.timePrice_N }} 円</td>
                </tr>
            </tbody>
        </table>
    </div>
</template>

<style scoped>
table {
  width: 100%;
  border-collapse: collapse;
}
th,
td {
  padding: 0.5rem;
  border: 1px solid #ccc;
  text-align: center;
}
h2 {
  margin-bottom: 1rem;
}
</style>