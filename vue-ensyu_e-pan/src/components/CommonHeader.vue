<script setup>
import { RouterLink, useRouter } from 'vue-router'

const router = useRouter();
function logout() {
  sessionStorage.clear();
  router.push('/');
}
</script>

<template>
  <header class="common-header no-print">
    <nav>
      <ul class="header-nav">
        <li><RouterLink to="/attendance-management">本日のシフト勤怠登録</RouterLink></li>
        <li><RouterLink to="/attendance-home">勤怠ホーム</RouterLink></li>
        <li><RouterLink to="/kibou-form">シフト希望フォーム</RouterLink></li>
        <li><RouterLink to="/saved-order">発注管理</RouterLink></li>
        <li><RouterLink to="/admin">アカウント管理</RouterLink></li>
        <li>
          <button class="logout-btn" @click="logout">ログアウト</button>
        </li>
      </ul>
    </nav>
  </header>
</template>

<style scoped>
.common-header {
  width: 100%;
  background-color: #f8f9fa;
  padding: 1rem 0;
  border-bottom: 1px solid #dee2e6;
  margin-bottom: 2rem;
}

.header-nav {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  justify-content: center;
  gap: 20px;
}

.header-nav a {
  text-decoration: none;
  color: #333;
  padding: 8px 15px;
  border-radius: 5px;
  transition: background-color 0.3s, color 0.3s;
}

.header-nav a:hover,
.header-nav a.router-link-active {
  background-color:  hsla(160, 100%, 37%, 1);
  color: white;
}

.header-nav li {
  display: flex;
  align-items: center;
}

.logout-btn {
  background-color: #df6c78;
  color: white;
  border: none;
  padding: 8px 15px;
  border-radius: 5px;
  cursor: pointer;
  margin-left: 5px;
  transition: background-color 0.3s;
}
.logout-btn:hover {
  background-color: #df6c78;
}
</style> 