import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import LoginPageView from '../views/LoginPageView.vue'
import AdminView from '../views/AdminView.vue'
import PartTimeView from '../views/PartTimeView.vue'
import AttendanceManagement from '../views/AttendanceManagement.vue'
import PurchaseOrder from '../views/PurchaseOrder.vue'


const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { //ホーム（デフォルトのVue機能）
      path: '/',
      name: 'home',
      component: HomeView,
    },
    { //アバウト（デフォルトのVue機能）
      path: '/about',
      name: 'about',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/AboutView.vue'),
    },
    { //ログイン表示
      path: '/login',
      name: 'login',
      component: LoginPageView,
    },
    { //アドミン。アカウント管理。社員一覧と新規登録
      path: '/admin',
      name: 'admin',
      component: AdminView,
      // meta: { requiresAuth: true, role: 'admin' },　ログイン機能
    },
    { ///???
      path: '/part-time',
      name: 'part-time',
      component: PartTimeView,
      // meta: { requiresAuth: true, role: 'partTime' }, ログイン機能
    },
    { //勤怠登録（当日）
      path: '/attendance-management',
      name: 'attendance-management',
      component: AttendanceManagement,
      // meta: { requiresAuth: true },　ログイン機能
    },
    { //勤怠ホーム
      path: '/attendance-home',
      name: 'attendance-home',
      component: () => import('../views/AttendanceHome.vue'),
      // meta: { requiresAuth: true },  ログイン機能
    },
    { //シフト希望提出フォーム
      path: '/kibou-form',
      name: 'kibou-form',
      component: () => import('../views/KibouForm.vue'),
      // meta: { requiresAuth: true }, //ログイン機能
    },
    { //
      path: '/purchase-order',
      name: 'PurchaseOrder',
      component: () => import('../views/PurchaseOrder.vue'),
      // meta: { requiresAuth: true }, ログイン機能
    },
    { //未整備？
      path: '/account',
      name: 'account',
      component: HomeView, // アカウント管理画面がないため仮でホームへ
      // meta: { requiresAuth: true },  ログイン機能
    },
     {//シフト調整フォーム
      path: '/Make-Attendance',
      name: 'Make-Attendance',
      component: () => import('../views/MakeAttendance.vue'),
    },
    {//シフト閲覧機能
      path: '/Check-Attendance',
      name: 'Check-Attendance',
      component: () => import('../views/CheckAttendance.vue'),
    },
     {//勤怠実績一覧
      path: '/Record-Attendance',
      name: 'Record-Attendance',
      component: () => import('../views/RecordAttendance.vue'),
    },
    {//勤怠編集画面（後日）
      path: '/Edit-Attendance',
      name: 'Edit-Attendance',
      component: () => import('../views/EditAttendance.vue'),
    },
  ],
})

router.beforeEach((to, from, next) => {
  const requiresAuth = to.matched.some((record) => record.meta.requiresAuth)
  const isAuthenticated = localStorage.getItem('token') // または他の認証状態の確認方法
  const userRole = localStorage.getItem('role') // ユーザーの役割

  if (requiresAuth && !isAuthenticated) {
    next('/login')
  } else if (isAuthenticated && to.name === 'login') {
    // ログインしているユーザーがログインページにアクセスしようとした場合
    next('/') // ホームにリダイレクト
  } else if (requiresAuth && to.meta.role && to.meta.role !== userRole) {
    // 必要な役割があるルートで、ユーザーの役割が一致しない場合
    next('/') // またはアクセス拒否ページへリダイレクト
  } else {
    next()
  }
})

export default router
