<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'https://localhost:5011/api', // 仮のバックエンドAPIのURL
  headers: {
    'Content-Type': 'application/json',
  },
});

const loginId = ref('');
const password = ref('');
const loginError = ref('');

const router = useRouter();

async function login() {
  loginError.value = '';
  try {
    const response = await apiClient.post('/auth/login', {
      loginId: loginId.value,
      password: password.value,
    });
    const { role, name, storeName } = response.data;
    
    // sessionStorageにユーザー情報を保存
    sessionStorage.setItem('userRole', role);
    sessionStorage.setItem('userName', name);
    sessionStorage.setItem('storeName', storeName);

    // 権限に応じてリダイレクト
    switch (role) {
      case 'admin':
        router.push('/admin');
        break;
      case 'partTime':
        router.push('/part-time');
        break;
      default:
        loginError.value = '不明なユーザー権限です。';
        sessionStorage.clear(); // 不正な場合はクリア
    }
  } catch (error) {
    if (error.response && (error.response.status === 401 || error.response.status === 404)) {
      loginError.value = 'ログインに失敗しました。IDまたはパスワードが間違っています。';
    } else {
      loginError.value = 'サーバーとの通信中にエラーが発生しました。';
      console.error('Login error:', error);
    }
  }
}

</script>

<template>
  <!-- ログイン画面 -->
  <div id="loginPage" class="container page">
    <h2>ログイン</h2>
    <input type="text" v-model="loginId" placeholder="ログインID">
    <input type="password" v-model="password" placeholder="パスワード">
    <button @click="login">ログイン</button>
    <p v-if="loginError" class="message">{{ loginError }}</p>
  </div>
</template>

<style scoped>
.container {
  max-width: 500px;
  margin: 2rem auto;
  padding: 2rem;
  border: 1px solid #ccc;
  border-radius: 8px;
}

.page h2 {
    text-align: center;
    margin-bottom: 1.5rem;
}

input, button {
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

.message {
  color: red;
  text-align: center;
  margin-top: 1rem;
}
</style>
