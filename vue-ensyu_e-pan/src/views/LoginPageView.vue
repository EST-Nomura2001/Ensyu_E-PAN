<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import axios from 'axios';
import CommonHeader from '../components/CommonHeader.vue';

const apiClient = axios.create({
  baseURL: 'http://localhost:5011/api', // 仮のバックエンドAPIのURL
  headers: {
    'Content-Type': 'application/json',
  },
});

const loginId = ref('');
const password = ref('');
const loginError = ref('');
const isAdmin = ref(false);

const router = useRouter();

onMounted(() => {
  isAdmin.value = sessionStorage.getItem('isAdmin') === 'true';
});

async function login() {
  loginError.value = '';
  try {
    const response = await apiClient.get('/Account/login', {
      params: {
        loginId: loginId.value,
        password: password.value,
      },
    });
    const data = response.data;
    // sessionStorageに必要な情報だけ保存
    sessionStorage.setItem('userId', data.id);
    sessionStorage.setItem('userName', data.name);
    sessionStorage.setItem('isAdmin', data.role?.isAdmin);
    sessionStorage.setItem('storeId', data.storeId ?? '');

    // sessionStorageの内容をコンソールに出力
    console.log('userId:', sessionStorage.getItem('userId'));
    console.log('userName:', sessionStorage.getItem('userName'));
    console.log('isAdmin:', sessionStorage.getItem('isAdmin'));
    console.log('storeId:', sessionStorage.getItem('storeId'));

    // 権限に応じてリダイレクト
    if (data.role?.isAdmin) {
      router.push('/admin');
    } else {
      router.push('/part-time');
    }
  } catch (error) {
    // コントローラーから返ってきたメッセージを表示
    if (error.response && error.response.data) {
      loginError.value = error.response.data;
    } else {
      loginError.value = 'サーバーとの通信中にエラーが発生しました。';
      console.error('Login error:', error);
    }
    sessionStorage.clear(); // 失敗時はクリア
  }
}

</script>

<template>
  <CommonHeader v-if="isAdmin" />
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
