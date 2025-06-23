import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import LoginPageView from '../views/LoginPageView.vue'
import AdminView from '../views/AdminView.vue'
import EmployeeView from '../views/EmployeeView.vue'
import PartTimeView from '../views/PartTimeView.vue'

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

    {//田村担当
      path: '/kibou-form',
      name: 'kibou-form',
      component: () => import('../views/KibouForm.vue'),
    },
     {//田村担当
      path: '/Attendance-home',
      name: 'Attendance-home',
      component: () => import('../views/AttendanceHome.vue'),
    },
  ],
})

router.beforeEach((to, from, next) => {
  const loggedInRole = sessionStorage.getItem('userRole');
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth);
  
  if (requiresAuth && !loggedInRole) {
    // 認証が必要なページに、未ログインでアクセスした場合
    next('/login');
  } else if (requiresAuth && to.meta.role && to.meta.role !== loggedInRole) {
    // 必要な権限と、実際の権限が異なる場合
    // とりあえずログインページにリダイレクトするが、権限エラーページなどを作っても良い
    next('/login');
  } else {
    next();
  }
});

export default router
