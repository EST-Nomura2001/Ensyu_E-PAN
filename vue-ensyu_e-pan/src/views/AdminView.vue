<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import axios from 'axios';
import CommonHeader from '@/components/CommonHeader.vue';

const apiClient = axios.create({
  baseURL: 'https://localhost:7017/api', // 仮のバックエンドAPIのURL
  headers: {
    'Content-Type': 'application/json',
  },
});

const router = useRouter();
const loggedInUserName = ref(sessionStorage.getItem('userName') || '');

const newUserId = ref('');
const newUserPw = ref('');
const newUserRole = ref('partTime');
const registerMsg = ref('');
const registerMsgIsSuccess = ref(false);

const userList = ref([]);

async function registerUser() {
  if (!newUserId.value || !newUserPw.value) {
    registerMsg.value = 'IDとパスワードを入力してください。';
    registerMsgIsSuccess.value = false;
    return;
  }
  try {
    await apiClient.post('/users/register', {
      loginId: newUserId.value,
      password: newUserPw.value,
      role: newUserRole.value,
    });
    registerMsg.value = `ユーザー「${newUserId.value}」を登録しました。`;
    registerMsgIsSuccess.value = true;
    newUserId.value = '';
    newUserPw.value = '';
    fetchUsers(); // 登録後にリストを更新
  } catch (error) {
    registerMsgIsSuccess.value = false;
    if (error.response && error.response.status === 409) {
      registerMsg.value = 'そのIDは既に使われています。';
    } else {
      registerMsg.value = 'ユーザー登録に失敗しました。';
      console.error('Register error:', error);
    }
  }
}

async function fetchUsers() {
  try {
    const response = await apiClient.get('/users');
    userList.value = response.data;
  } catch (error) {
    console.error('Failed to fetch users:', error);
  }
}

const userListForDisplay = computed(() => {
  return userList.value.map(user => {
    const roleLabel = user.role === 'partTime' ? 'アルバイト'
                    : user.role === 'admin' ? '管理者'
                    : '不明';
    return { id: user.id, label: `・${user.loginId}（${roleLabel}）` };
  });
});

function logout() {
  sessionStorage.removeItem('userRole');
  sessionStorage.removeItem('userName');
  router.push('/login');
}

onMounted(() => {
  fetchUsers();
});

</script>

<template>
  <CommonHeader />
  <div id="adminPage" class="container page">
    <h2>管理者ページ</h2>
    <p>ようこそ、{{ loggedInUserName }}さん (管理者)！</p>
    <p>新しいユーザーを登録できます。</p>

    <input type="text" v-model="newUserId" placeholder="新規ユーザーID">
    <input type="password" v-model="newUserPw" placeholder="パスワード">
    <select v-model="newUserRole">
      <option value="partTime">アルバイト</option>
      <option value="admin">管理者</option>
    </select>
    <button @click="registerUser">登録</button>
    <p v-if="registerMsg" class="message" :class="{ success: registerMsgIsSuccess }">{{ registerMsg }}</p>

    <router-link to="/attendance-management" custom v-slot="{ navigate }">
      <button @click="navigate" role="link" class="nav-button">勤怠管理ページへ</button>
    </router-link>

    <h3>登録済みユーザー一覧</h3>
    <div class="user-list">
      <p v-if="userList.length === 0">現在登録されているユーザーはいません。</p>
      <p v-for="user in userListForDisplay" :key="user.id">{{ user.label }}</p>
    </div>

    <button class="logout-btn" @click="logout">ログアウト</button>
  </div>
</template>

<style scoped>
.container {
  max-width: 800px;
  margin: 2rem auto;
  padding: 2rem;
  border: 1px solid #ccc;
  border-radius: 8px;
}

.page h2 {
    text-align: center;
    margin-bottom: 1.5rem;
}

input, select, button {
  display: block;
  width: 100%;
  padding: 0.75rem;
  margin-bottom: 1rem;
  box-sizing: border-box;
  border-radius: 4px;
  border: 1px solid #ccc;
}

button {
  background-color: #007bff;
  color: white;
  border: none;
  cursor: pointer;
  transition: background-color 0.2s;
}

button:hover {
    background-color: #0056b3;
}

.logout-btn {
    background-color: #dc3545;
}

.logout-btn:hover {
    background-color: #c82333;
}

.message {
  color: red;
  text-align: center;
  margin-top: 1rem;
}

.message.success {
  color: green;
}

.user-list {
    margin-top: 1.5rem;
    padding-top: 1rem;
    border-top: 1px solid #eee;
}

.nav-button {
    background-color: #28a745;
    margin-top: 1rem;
}

.nav-button:hover {
    background-color: #218838;
}
</style> 