import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import LoginPageView from '../views/LoginPageView.vue'
import AdminView from '../views/AdminView.vue'
import EmployeeView from '../views/EmployeeView.vue'
import PartTimeView from '../views/PartTimeView.vue'
import AttendanceManagement from '../views/AttendanceManagement.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/about',
      name: 'about',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/AboutView.vue'),
    },
    {
      path: '/login',
      name: 'login',
      component: LoginPageView,
    },
    {
      path: '/admin',
      name: 'admin',
      component: AdminView,
      meta: { requiresAuth: true, role: 'admin' },
    },
    {
      path: '/employee',
      name: 'employee',
      component: EmployeeView,
      meta: { requiresAuth: true, role: 'employee' },
    },
    {
      path: '/part-time',
      name: 'part-time',
      component: PartTimeView,
      meta: { requiresAuth: true, role: 'partTime' },
    },
    {
      path: '/attendance-management',
      name: 'attendance-management',
      component: AttendanceManagement,
      meta: { requiresAuth: true },
    },
    {
      path: '/attendance',
      name: 'attendance',
      component: () => import('../views/AttendanceHome.vue'), // 仮で勤怠ホームへ
      meta: { requiresAuth: true },
    },
    {
      path: '/attendance-home',
      name: 'attendance-home',
      component: () => import('../views/AttendanceHome.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/kibou-form',
      name: 'kibou-form',
      component: () => import('../views/KibouForm.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/purchase-order',
      name: 'purchase-order',
      component: () => import('../views/PurchaseOrder.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/account',
      name: 'account',
      component: HomeView, // アカウント管理画面がないため仮でホームへ
      meta: { requiresAuth: true },
    },
     {//田村担当
      path: '/Make-Attendance',
      name: 'Make-Attendance',
      component: () => import('../views/MakeAttendance.vue'),
    },
    {//田村担当
      path: '/Check-Attendance',
      name: 'Check-Attendance',
      component: () => import('../views/CheckAttendance.vue'),
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
