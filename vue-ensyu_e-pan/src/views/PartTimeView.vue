<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import CommonHeader from '@/components/CommonHeader.vue';

const router = useRouter();
const loggedInUserName = ref(sessionStorage.getItem('userName') || '');

function logout() {
  sessionStorage.removeItem('userRole');
  sessionStorage.removeItem('userName');
  router.push('/');
}
</script>

<template>
  
  <div id="partTimePage" class="container page">
    <h2>アルバイトページ</h2>
    <p>ようこそ、{{ loggedInUserName }}さん (アルバイト)！</p>

    <router-link to="/attendance-management" custom v-slot="{ navigate }">
      <button @click="navigate" role="link" class="nav-button">勤怠管理ページへ</button>
    </router-link>

    <router-link to="/kibou-form" custom v-slot="{ navigate }">
      <button @click="navigate" role="link" class="nav-button">シフト希望フォームへ</button>
    </router-link>

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

.nav-button {
    display: block;
    width: 100%;
    padding: 0.75rem;
    margin-bottom: 1rem;
    box-sizing: border-box;
    border-radius: 4px;
    background-color: #28a745;
    color: white;
    border: none;
    cursor: pointer;
    transition: background-color 0.2s;
}

.nav-button:hover {
    background-color: #218838;
}

.logout-btn {
    display: block;
    width: 100%;
    padding: 0.75rem;
    margin-top: 1.5rem;
    box-sizing: border-box;
    border-radius: 4px;
    background-color: #dc3545;
    color: white;
    border: none;
    cursor: pointer;
    transition: background-color 0.2s;
}

.logout-btn:hover {
    background-color: #c82333;
}
</style> 