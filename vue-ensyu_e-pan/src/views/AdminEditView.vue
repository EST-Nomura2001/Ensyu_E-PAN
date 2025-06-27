<script setup>
import axios from 'axios'
import { ref, reactive, onMounted } from 'vue'
import CommonHeader from '@/components/CommonHeader.vue';

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
    login_Id: user.login_Id,
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
});
</script>

<template>
    <CommonHeader/>
  <div class="container">
    <!-- ヘッダー -->
    <header class="header">
      <div class="header-content">
        <h1 class="title">
          <svg class="title-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
            <circle cx="12" cy="7" r="4"/>
          </svg>
          アカウント管理
        </h1>
        <div class="user-count">
          <span class="count-badge">{{ users.length }} 名のユーザー</span>
        </div>
      </div>
    </header>

    <!-- メインコンテンツ -->
    <main class="main-content">
      <!-- ユーザーリスト -->
      <div class="users-grid">
        <div v-for="user in users" :key="user.id" class="user-card">
          <div class="user-card-header">
            <div class="user-avatar">
              <span class="avatar-text">{{ user.name.charAt(0) }}</span>
            </div>
            <div class="user-info">
              <h3 class="user-name">{{ user.name }}</h3>
              <p class="user-login-id">@{{ user.login_Id }}</p>
            </div>
            <div class="user-badges">
              <span v-if="user.isAdmin" class="badge admin-badge">管理者</span>
            </div>
          </div>
          
          <div class="user-details">
            <div class="detail-item">
              <span class="detail-label">役職</span>
              <span class="detail-value">{{ user.rollName }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">店舗</span>
              <span class="detail-value">{{ user.storeName }}</span>
            </div>
            <div class="salary-section">
              <div class="salary-item">
                <span class="salary-label">日勤</span>
                <span class="salary-value">¥{{ user.timePrice_D.toLocaleString() }}</span>
              </div>
              <div class="salary-item">
                <span class="salary-label">夜勤</span>
                <span class="salary-value">¥{{ user.timePrice_N.toLocaleString() }}</span>
              </div>
            </div>
          </div>

          <div class="user-actions">
            <button @click="startEdit(user)" class="btn btn-edit">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/>
                <path d="m18.5 2.5 a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/>
              </svg>
              編集
            </button>
            <button @click="deleteUser(user.id)" class="btn btn-delete">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polyline points="3,6 5,6 21,6"/>
                <path d="m19,6v14a2,2 0 0,1 -2,2H7a2,2 0 0,1 -2,-2V6m3,0V4a2,2 0 0,1 2,-2h4a2,2 0 0,1 2,2v2"/>
              </svg>
              削除
            </button>
          </div>
        </div>
      </div>

      <!-- 編集モーダル -->
      <div v-if="isEditing" class="modal-overlay" @click="isEditing = false">
        <div class="modal" @click.stop>
          <div class="modal-header">
            <h2 class="modal-title">アカウント編集</h2>
            <button @click="isEditing = false" class="modal-close">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <line x1="18" y1="6" x2="6" y2="18"/>
                <line x1="6" y1="6" x2="18" y2="18"/>
              </svg>
            </button>
          </div>
          
          <div class="modal-content">
            <div class="form-grid">
              <div class="form-group">
                <label class="form-label">ログインID</label>
                <input v-model="editingUser.login_Id" class="form-input" placeholder="ログインIDを入力" />
              </div>
              
              <div class="form-group">
                <label class="form-label">氏名</label>
                <input v-model="editingUser.name" class="form-input" placeholder="氏名を入力" />
              </div>
              
              <div class="form-group">
                <label class="form-label">パスワード</label>
                <input type="password" v-model="editingUser.password" class="form-input" placeholder="新しいパスワードを入力" />
              </div>
              
              <div class="form-group">
                <label class="form-label">役職</label>
                <select v-model.number="editingUser.roll_Cd" class="form-select">
                  <option v-for="roll in rolls" :key="roll.id" :value="roll.id">{{ roll.name }}</option>
                </select>
              </div>
              
              <div class="form-group">
                <label class="form-label">店舗</label>
                <select v-model.number="editingUser.stores_Cd" class="form-select">
                  <option v-for="store in stores" :key="store.id" :value="store.id">{{ store.c_Name }}</option>
                </select>
              </div>
              
              <div class="form-group">
                <label class="form-label">日勤時給 (円)</label>
                <input type="number" min="0" v-model.number="editingUser.timePrice_D" class="form-input" placeholder="0" />
              </div>
              
              <div class="form-group">
                <label class="form-label">夜勤時給 (円)</label>
                <input type="number" min="0" v-model.number="editingUser.timePrice_N" class="form-input" placeholder="0" />
              </div>
            </div>
          </div>

          <div class="modal-footer">
            <button @click="isEditing = false" class="btn btn-secondary">キャンセル</button>
            <button @click="updateUser" class="btn btn-primary">更新</button>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>

<style scoped>
* {
  box-sizing: border-box;
}

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  margin: 0;
  padding: 0;
  background: #fafafa;
  min-height: 100vh;
}

.container {
  min-height: 100vh;
  background: #fafafa;
  padding: 1rem;
}

/* ヘッダー */
.header {
  margin-bottom: 2rem;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: white;
  border-radius: 16px;
  padding: 2rem;
  border: 1px solid #e5e7eb;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.title {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin: 0;
  color: #111827;
  font-size: 2rem;
  font-weight: 700;
}

.title-icon {
  width: 2rem;
  height: 2rem;
  color: #111827;
}

.count-badge {
  background: #111827;
  color: white;
  padding: 0.5rem 1rem;
  border-radius: 8px;
  font-weight: 600;
  font-size: 0.875rem;
}

/* メインコンテンツ */
.main-content {
  max-width: 1400px;
  margin: 0 auto;
}

/* ユーザーグリッド */
.users-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 1.5rem;
}

.user-card {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  border: 1px solid #e5e7eb;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  transition: all 0.2s ease;
}

.user-card:hover {
  border-color: #111827;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.user-card-header {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.user-avatar {
  width: 3rem;
  height: 3rem;
  border-radius: 50%;
  background: #111827;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.avatar-text {
  color: white;
  font-weight: 700;
  font-size: 1.25rem;
}

.user-info {
  flex: 1;
  min-width: 0;
}

.user-name {
  margin: 0 0 0.25rem 0;
  font-size: 1.25rem;
  font-weight: 700;
  color: #111827;
}

.user-login-id {
  margin: 0;
  color: #6b7280;
  font-size: 0.875rem;
}

.user-badges {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.badge {
  padding: 0.25rem 0.75rem;
  border-radius: 50px;
  font-size: 0.75rem;
  font-weight: 600;
  text-align: center;
}

.admin-badge {
  background: #111827;
  color: white;
}

/* ユーザー詳細 */
.user-details {
  margin-bottom: 1.5rem;
}

.detail-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.5rem 0;
  border-bottom: 1px solid #f3f4f6;
}

.detail-item:last-child {
  border-bottom: none;
}

.detail-label {
  color: #6b7280;
  font-size: 0.875rem;
  font-weight: 500;
}

.detail-value {
  color: #111827;
  font-weight: 600;
}

.salary-section {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #f3f4f6;
}

.salary-item {
  background: #f9fafb;
  padding: 0.75rem;
  border-radius: 8px;
  text-align: center;
  border: 1px solid #f3f4f6;
}

.salary-label {
  display: block;
  color: #6b7280;
  font-size: 0.75rem;
  font-weight: 600;
  margin-bottom: 0.25rem;
}

.salary-value {
  color: #111827;
  font-size: 1.125rem;
  font-weight: 700;
}

/* アクションボタン */
.user-actions {
  display: flex;
  gap: 0.75rem;
}

.btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1rem;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  font-size: 0.875rem;
  cursor: pointer;
  transition: all 0.2s ease;
  flex: 1;
  justify-content: center;
}

.btn svg {
  width: 1rem;
  height: 1rem;
}

.btn-edit {
  background: #111827;
  color: white;
  border: 1px solid #111827;
}

.btn-edit:hover {
  background: #374151;
  border-color: #374151;
}

.btn-delete {
  background: white;
  color: #dc2626;
  border: 1px solid #dc2626;
}

.btn-delete:hover {
  background: #dc2626;
  color: white;
}

/* モーダル */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
}

.modal {
  background: white;
  border-radius: 12px;
  width: 100%;
  max-width: 800px;
  max-height: 90vh;
  overflow: auto;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
  border: 1px solid #e5e7eb;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 2rem 2rem 0 2rem;
  margin-bottom: 1.5rem;
  border-bottom: 1px solid #f3f4f6;
  padding-bottom: 1rem;
}

.modal-title {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 700;
  color: #111827;
}

.modal-close {
  background: none;
  border: none;
  padding: 0.5rem;
  cursor: pointer;
  border-radius: 6px;
  transition: background-color 0.2s ease;
}

.modal-close:hover {
  background: #f3f4f6;
}

.modal-close svg {
  width: 1.25rem;
  height: 1.25rem;
  color: #6b7280;
}

.modal-content {
  padding: 0 2rem;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
}

.form-label {
  margin-bottom: 0.5rem;
  font-weight: 600;
  color: #111827;
  font-size: 0.875rem;
}

.form-input, .form-select {
  padding: 0.875rem 1rem;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 0.875rem;
  transition: all 0.2s ease;
  background: white;
}

.form-input:focus, .form-select:focus {
  outline: none;
  border-color: #111827;
  box-shadow: 0 0 0 3px rgba(17, 24, 39, 0.1);
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding: 2rem;
  border-top: 1px solid #f3f4f6;
  margin-top: 2rem;
}

.btn-primary {
  background: #111827;
  color: white;
  border: 1px solid #111827;
}

.btn-primary:hover {
  background: #374151;
  border-color: #374151;
}

.btn-secondary {
  background: white;
  color: #6b7280;
  border: 1px solid #d1d5db;
}

.btn-secondary:hover {
  background: #f9fafb;
  border-color: #9ca3af;
}

/* レスポンシブ */
@media (max-width: 768px) {
  .container {
    padding: 0.5rem;
  }
  
  .header-content {
    flex-direction: column;
    gap: 1rem;
    text-align: center;
  }
  
  .title {
    font-size: 1.5rem;
  }
  
  .users-grid {
    grid-template-columns: 1fr;
  }
  
  .form-grid {
    grid-template-columns: 1fr;
  }
  
  .modal {
    margin: 0.5rem;
  }
  
  .modal-header, .modal-content, .modal-footer {
    padding-left: 1rem;
    padding-right: 1rem;
  }
}
</style>