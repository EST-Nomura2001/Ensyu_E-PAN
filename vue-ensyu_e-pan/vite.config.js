import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig({
  server: {
    proxy: {
      // '/api' という文字列を含むリクエストをプロキシします
      '/api': {
        // 転送先を指定します
        // このURLはご自身のASP.NET Web APIの実行環境に合わせて変更してください
        target: 'https://localhost:7048',
        // オリジンを変更してCORSエラーを回避します
        changeOrigin: true,
        // HTTPSの証明書検証を無効にします (自己署名証明書の場合に必要)
        secure: false,
        // パスから '/api' を削除します
        rewrite: (path) => path.replace(/^\/api/, ''),
      },
    },
  },
  plugins: [
    vue(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
})
