<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const loggedInUserName = ref(sessionStorage.getItem('userName') || '');

function logout() {
  sessionStorage.removeItem('userRole');
  sessionStorage.removeItem('userName');
  router.push('/login');
}
</script>

<template>
  <div id="partTimePage" class="container page">
    <h2>アルバイトページ</h2>
    <p>ようこそ、{{ loggedInUserName }}さん (アルバイト)！</p>
    <button class="logout-btn" @click="logout">ログアウト</button>
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