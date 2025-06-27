<script setup>
import axios from 'axios'
import { ref, reactive, onMounted } from 'vue'

const users = ref([]);
const rolls = ref([]);//役職一覧
const stores = ref([]);//店舗一覧



const editingUser = reactive({
  id: null,
  login_Id: '',
  name: '',
  password: '',
  roll_Cd: 1,
  stores_Cd: 1,
  timePrice_D: 0,
  timePrice_N: 0
})

const isEditing = ref(false)

async function fetchUsers() {
  try {
    const res = await axios.get('http://localhost:5011/api/Account/users/all')
    users.value = res.data
  } catch (e) {
    console.error('取得エラー:', e)
  }
}

async function deleteUser(id) {
const targetUser = users.value.find(user => user.id === id)


// 削除対象が管理者かチェック
if (targetUser?.isAdmin) {
// 管理者の人数を数える
    const adminCount = users.value.filter(user => user.isAdmin).length
    if (adminCount <= 1) {
    alert('管理者ユーザーが0人になるため削除できません。最低1名の管理者が必要です。')
    return
    }
}
if (confirm(`ID: ${id} を削除しますか？`)) {
    try {
    await axios.delete(`http://localhost:5011/api/Account/users/${id}`)
    await fetchUsers()
    } catch (e) {
    console.error('削除失敗:', e)
    }
}
}

function startEdit(user) {
isEditing.value = true
Object.assign(editingUser, {
    id: user.id,
    login_Id: user.login_Id, // ← 追加
    name: user.name,
    password: '',
    roll_Cd: user.roll_Cd,
    stores_Cd: user.stores_Cd,
    timePrice_D: user.timePrice_D,
    timePrice_N: user.timePrice_N
})
}

async function updateUser() {
    const adminRoleCds = [1, 2]; // 管理者とみなすロールCD
    console.log('更新データ:', {
        Login_Id: editingUser.login_Id,
        Roll_Cd: editingUser.roll_Cd,
        Stores_Cd: editingUser.stores_Cd
    })
    if (
        !editingUser.login_Id.trim() ||
        !editingUser.name.trim() ||
        !editingUser.password.trim() ||
        !editingUser.roll_Cd ||
        !editingUser.stores_Cd
    ) {
        alert('すべての必須項目（ログインID、氏名、パスワード、ロールCD、店舗CD）を入力してください。')
        return
    }
    if(editingUser.timePrice_D <=0 || editingUser.timePrice_N <=0){
        alert('給料は支給してあげてください……')
        return
    }
    // 編集前のユーザー情報を取得
    const originalUser = users.value.find(user => user.id === editingUser.id);
    const isOriginalAdmin = originalUser && adminRoleCds.includes(originalUser.roll_Cd);
    const isUpdatedAdmin = adminRoleCds.includes(editingUser.roll_Cd);

    // 管理者が0人になる可能性がある場合、更新をブロック
    if (isOriginalAdmin && !isUpdatedAdmin) {
        const currentAdminCount = users.value.filter(user => adminRoleCds.includes(user.roll_Cd)).length;

        if (currentAdminCount <= 1) {
        alert('この操作により管理者がいなくなります。少なくとも1名の管理者を残してください。');
        return;
        }
    }

try {
    await axios.put(`http://localhost:5011/api/Account/users/${editingUser.id}`, {
        Login_Id: editingUser.login_Id,
        Name: editingUser.name,
        Password: editingUser.password,
        Roll_Cd: editingUser.roll_Cd,
        Stores_Cd: editingUser.stores_Cd,
        TimePrice_D: editingUser.timePrice_D,
        TimePrice_N: editingUser.timePrice_N
    })
    isEditing.value = false
    await fetchUsers()
    } catch (e) {
        console.error('更新失敗:', e)
    }
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

onMounted(()=>{
    fetchUsers();
    getRolls();
    getStores();
}
);
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
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="user in users" :key="user.id">
          <td>{{ user.id }}</td>
          <td>{{ user.login_Id }}</td>
          <td>{{ user.name }}</td>
          <td>{{ user.rollName }}</td>
          <td>{{ user.isAdmin ? '有' : '無' }}</td>
          <td>{{ user.storeName }}</td>
          <td>{{ user.timePrice_D }} 円</td>
          <td>{{ user.timePrice_N }} 円</td>
          <td>
            <button @click="startEdit(user)">編集</button>
            <button @click="deleteUser(user.id)">削除</button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- 編集フォーム -->
    <div v-if="isEditing" style="margin-top: 2rem; border-top: 1px solid #ccc; padding-top: 1rem;">
    <h3>アカウント編集</h3>
    <label>ログインID: <input v-model="editingUser.login_Id" /></label><br />
    <label>氏名: <input v-model="editingUser.name" /></label><br />
    <label>パスワード: <input type="password" v-model="editingUser.password" /></label><br />
    <label>ロールCD: <select v-model.number="editingUser.roll_Cd">
        <option v-for="roll in rolls" :key="roll.id" :value="roll.id">{{ roll.name }}</option>
    </select></label><br />
    <label>
        店舗CD: <select v-model.number="editingUser.stores_Cd">
            <option v-for="store in stores" :value="store.id">{{store.c_Name}}</option>
        </select>
    </label><br />
    <label>日勤時給: <input type="number" min="0" v-model.number="editingUser.timePrice_D" /></label><br />
    <label>夜勤時給: <input type="number" min="0" v-model.number="editingUser.timePrice_N" /></label><br />
    <button @click="updateUser">更新</button>
    <button @click="isEditing = false">キャンセル</button>
    </div>
  </div>
</template>