<script setup>
import { RouterLink } from 'vue-router'
</script>

<template>
  <header class="common-header">
    <nav>
      <ul class="header-nav">
        <li><RouterLink to="/attendance">本日のシフト勤怠登録</RouterLink></li>
        <li><RouterLink to="/attendance-home">勤怠ホーム</RouterLink></li>
        <li><RouterLink to="/kibou-form">シフト希望フォーム</RouterLink></li>
        <li><RouterLink to="/purchase-order">発注管理</RouterLink></li>
        <li><RouterLink to="/account">アカウント管理</RouterLink></li>
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
  background-color: #000000;
  color: white;
}
</style> 