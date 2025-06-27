<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import axios from 'axios';
import CommonHeader from '@/components/CommonHeader.vue';

const apiClient = axios.create({
  baseURL: 'http://localhost:5011/api', // 仮のバックエンドAPIのURL
  headers: {
    'Content-Type': 'application/json',
  },
});

const router = useRouter();
const loggedInUserName = ref(sessionStorage.getItem('userName') || '');

const newUserId = ref('');
const newUserName = ref('');
const newUserPw = ref('');
const newUserRole = ref(1);
const newUserStore = ref(1);
const dayTimePrace = ref(0);
const nightTimePrace = ref(0);

const registerMsg = ref('');
const registerMsgIsSuccess = ref(false);

const userList = ref([]);
const rolls = ref([]);//役職一覧
const stores = ref([]);//店舗一覧

//登録処理
async function registerUser() {
  if (!newUserId.value || !newUserPw.value) {
    registerMsg.value = 'IDとパスワードを入力してください。';
    registerMsgIsSuccess.value = false;
    return;
  }
  if(newUserRole.value == null){
    registerMsg.value = '役職を選択してください';
    registerMsgIsSuccess.value = false;
    return;
  }
  if(!newUserName.value){
    registerMsg.value = '名前を入力してください';
    registerMsgIsSuccess.value = false;
    return;
  }
  if(dayTimePrace.value <= 0 || nightTimePrace <= 0){
    registerMsg.value = '給料は払いましょうよ……';
    registerMsgIsSuccess.value = false;
    return;
  }
  try {
    await apiClient.post('http://localhost:5011/api/Account/users/register', {
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
    const response = await apiClient.get('/Account/users/all');
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
  sessionStorage.clear(); // すべてのsessionStorageデータをクリア
  router.push('/login');
}

//役職一覧作成
const getRolls = async()=>{
  const rollResponse = await axios.get('http://localhost:5011/api/Masters/roles');
  console.log("役職一覧",rollResponse.data);
  rolls.value = rollResponse.data;
};

//店舗一覧作成
const getStores = async()=>{
  const storeResponse = await axios.get('http://localhost:5011/api/Masters/stores');
  console.log("役職一覧",storeResponse.data);
  stores.value = storeResponse.data;
};

onMounted(() => {
  fetchUsers();
  getRolls();
  getStores();
});

</script>

<template>
  <CommonHeader />
  <div id="adminPage" class="container page">
    <h2>管理者ページ</h2>
    <p>ようこそ、{{ loggedInUserName }}さん (管理者)！</p>
    <p>新しいユーザーを登録できます。</p>

    <input type="text" v-model="newUserId" placeholder="新規ユーザーID">
    <input type="text" placeholder="新規ユーザー名" v-model = "newUserName">
    <input type="password" v-model="newUserPw" placeholder="パスワード">
    <select v-model="newUserRole">
      <option v-for="roll in rolls" :key="roll.id" :value="roll.id">{{ roll.name }}</option>
    </select>
    <select v-model="newUserStore">
      <option v-for="store in stores" :value="store.id">{{store.c_Name}}</option>
    </select>
    <div>
      <p>日中時給<input type="number" min="0" v-model="dayTimePrace"></p>
      <p>深夜時給<input type="number" min="0" v-model="nightTimePrace"></p>
    </div>
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