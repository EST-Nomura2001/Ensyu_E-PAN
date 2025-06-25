<template>
  <CommonHeader />
  <!-- 保存済み発注書一覧のメインコンテナ -->
  <div class="saved-orders-container">
    <section class="saved-orders-section">
      <!-- ページのタイトル -->
      <h2>保存済み発注書一覧</h2>
      
      <!-- アクションボタンエリア（新規作成ボタンと更新ボタン） -->
      <div class="actions">
        <!-- 新規発注書作成ページへのリンク -->
        <router-link to="/" class="action-btn new-order-btn">＋ 新規発注書を作成</router-link>
        <!-- 一覧を再読み込みするボタン -->
        <button @click="fetchOrders" class="refresh-btn">一覧を更新</button>
      </div>
      
      <!-- テーブルを横スクロール可能にするラッパー -->
      <div class="table-wrapper">
        <!-- 発注書一覧を表示するテーブル -->
        <table class="saved-orders-table">
          <!-- テーブルのヘッダー行 -->
          <thead>
            <tr>
              <th>発注番号</th>
              <th>件名</th>
              <th>宛名</th>
              <th>発注日</th>
              <th>保存日時</th>
              <th class="col-confirm">確定</th>
            </tr>
          </thead>
          
          <!-- テーブルのデータ行 -->
          <tbody>
            <!-- データ読み込み中の表示 -->
            <tr v-if="isLoading">
              <td colspan="6">読み込み中...</td>
            </tr>
            
            <!-- 保存された発注書が0件の場合の表示 -->
            <tr v-else-if="savedOrders.length === 0">
              <td colspan="6">保存された発注書はありません。</td>
            </tr>
            
            <!-- 保存された発注書を1行ずつ表示 -->
            <!-- v-for: 配列の各要素に対して繰り返し処理 -->
            <!-- :key: Vue.jsがリストの各要素を識別するための一意なキー -->
            <!-- @click: 行をクリックした時の処理 -->
            <!-- :class: 条件によってCSSクラスを動的に適用 -->
            <tr v-for="order in savedOrders" :key="order.id" 
                @click="navigateToPurchaseOrder(order.id)" 
                class="order-row"
                :class="{ 'order-confirmed': order.isConfirmed }">
              <!-- 各データを表示 -->
              <td>{{ order.Quotation }}</td>
              <td>{{ order.Title }}</td>
              <td>{{ order.Company.C_Name }}</td>
              <!-- 日付フォーマット関数を使用 -->
              <td>{{ formatDate(order.Order_Date) }}</td>
              <td>{{ formatDateTime(order.createdAt) }}</td>
              <!-- 確定済みの場合のみチェックマークを表示 -->
              <td class="col-confirm">
                <span v-if="order.isConfirmed" class="confirmed-mark">✔</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </div>
</template>

<script>
// 必要なライブラリをインポート
import axios from 'axios'; // HTTP通信用ライブラリ
import { useRouter } from 'vue-router'; // ページ遷移用

//ヘッダー用
import CommonHeader from '../components/CommonHeader.vue';

// APIのベースURL（サーバーのアドレス）
const API_BASE_URL = 'http://localhost:5011';

export default {
  // コンポーネント名
  name: 'SavedOrders',
  
  // Vue 3のComposition APIを使用した初期設定
  setup() {
    // ルーター（ページ遷移）の機能を取得
    const router = useRouter();
    
    // 発注書詳細ページに遷移する関数
    const navigateToPurchaseOrder = (orderId) => {
      // クエリパラメータとしてIDを渡して発注書ページに遷移
      // 例: /purchase-order?id=123 のようなURLになる
      router.push({ name: 'PurchaseOrder', query: { id: orderId } });
    };

    // setup関数で定義した関数をコンポーネントで使えるように返す
    return { navigateToPurchaseOrder };
  },
  
  // コンポーネントのデータ（状態）を定義
  data() {
    return {
      savedOrders: [],    // 保存された発注書のリスト
      isLoading: false,   // データ読み込み中かどうかのフラグ
    };
  },
  
  // コンポーネントが作成された時に実行される処理
  created() {
    // ページが表示されたら発注書一覧を取得
    this.fetchOrders();
  },
  
  // コンポーネントのメソッド（関数）を定義
  methods: {
    // サーバーから発注書一覧を取得する関数
    async fetchOrders() {
      // 読み込み中フラグをONにする
      this.isLoading = true;
      
      try {
        // サーバーから発注書一覧のデータを取得
        const response = await axios.get(`${API_BASE_URL}/api/orders`);
        // 取得したデータをコンポーネントの状態に保存
        this.savedOrders = response.data;
      } catch (error) {
        // エラーが発生した場合の処理
        console.error('発注書一覧の取得に失敗しました:', error);
        alert('発注書一覧の取得に失敗しました。');
      } finally {
        // 成功・失敗に関係なく最後に実行される処理
        // 読み込み中フラグをOFFにする
        this.isLoading = false;
      }
    },
    
    // 日付を日本語形式（YYYY/MM/DD）でフォーマットする関数
    formatDate(dateString) {
      // 日付文字列が空の場合は空文字を返す
      if (!dateString) return '';
      
      // 文字列をDateオブジェクトに変換
      const date = new Date(dateString);
      // 日本語ロケールで日付をフォーマット
      return date.toLocaleDateString('ja-JP');
    },
    
    // 日時を日本語形式（YYYY/MM/DD HH:MM）でフォーマットする関数
    formatDateTime(dateString) {
      // 日付文字列が空の場合は空文字を返す
      if (!dateString) return '';
      
      // 文字列をDateオブジェクトに変換
      const date = new Date(dateString);
      // 日本語ロケールで日時をフォーマット
      return date.toLocaleString('ja-JP', { 
        year: 'numeric',    // 年を4桁で表示
        month: '2-digit',   // 月を2桁で表示
        day: '2-digit',     // 日を2桁で表示
        hour: '2-digit',    // 時を2桁で表示
        minute: '2-digit'   // 分を2桁で表示
      });
    }
  },
  
  //ヘッダー用
  components: {
    CommonHeader
  },
};
</script>

<style scoped>
/* scopedによりこのコンポーネント内でのみ適用されるスタイル */

/* メインコンテナのスタイル */
.saved-orders-container {
  padding: 20px;                    /* 内側の余白 */
  background-color: #f9f9f9;        /* 背景色（薄いグレー） */
  min-height: 100vh;                /* 最小の高さを画面の高さに設定 */
}

/* 発注書セクションのスタイル */
.saved-orders-section {
  max-width: 1000px;                /* 最大幅を1000pxに制限 */
  margin: 20px auto;                /* 上下20px、左右は中央揃え */
  padding: 25px;                    /* 内側の余白 */
  background-color: #fff;           /* 背景色（白） */
}

/* タイトル（h2）のスタイル */
.saved-orders-section h2 {
  margin-top: 0;                    /* 上の余白を0に */
  margin-bottom: 20px;              /* 下の余白を20pxに */
  border-bottom: 2px solid #007bff; /* 下に青い線を引く */
  padding-bottom: 10px;             /* 下の内側余白 */
  font-size: 22px;                  /* 文字サイズ */
  color: #333;                      /* 文字色（濃いグレー） */
}

/* アクションボタンエリアのスタイル */
.actions {
  display: flex;                    /* フレックスボックスレイアウト */
  justify-content: space-between;   /* 要素を両端に配置 */
  align-items: center;              /* 垂直方向の中央揃え */
  margin-bottom: 20px;              /* 下の余白 */
}

/* ボタン共通のスタイル */
.action-btn, .refresh-btn {
  padding: 10px 20px;               /* 内側の余白 */
  border: none;                     /* 境界線なし */
  border-radius: 5px;               /* 角を丸くする */
  cursor: pointer;                  /* マウスオーバー時にポインターカーソル */
  font-size: 15px;                  /* 文字サイズ */
  font-weight: bold;                /* 文字を太字に */
  text-decoration: none;            /* リンクの下線を消す */
  display: inline-block;            /* インラインブロック要素に */
  text-align: center;               /* 文字を中央揃え */
}

/* 新規作成ボタンのスタイル */
.new-order-btn {
  background-color: #28a745;        /* 背景色（緑） */
  color: white;                     /* 文字色（白） */
}
/* 新規作成ボタンのホバー時のスタイル */
.new-order-btn:hover {
  background-color: #218838;        /* より濃い緑に変更 */
}

/* 更新ボタンのスタイル */
.refresh-btn {
  background-color: #f8f9fa;        /* 背景色（薄いグレー） */
  border: 1px solid #ccc;           /* 境界線（グレー） */
  color: #333;                      /* 文字色（濃いグレー） */
}
/* 更新ボタンのホバー時のスタイル */
.refresh-btn:hover {
  background-color: #e2e6ea;        /* 少し濃いグレーに変更 */
}

/* テーブルラッパーのスタイル */
.table-wrapper {
  overflow-x: auto;                 /* 横方向にスクロール可能にする */
}

/* テーブル全体のスタイル */
.saved-orders-table {
  width: 100%;                      /* 幅を100%に */
  border-collapse: collapse;        /* 境界線を結合 */
  font-size: 14px;                  /* 文字サイズ */
}

/* テーブルのセル（th, td）共通スタイル */
.saved-orders-table th, .saved-orders-table td {
  border: 1px solid #ddd;           /* 境界線（薄いグレー） */
  padding: 12px 15px;               /* 内側の余白 */
  text-align: left;                 /* 文字を左揃え */
  white-space: nowrap;              /* テキストの改行を防ぐ */
}

/* テーブルヘッダーのスタイル */
.saved-orders-table thead {
  background-color: #e9ecef;        /* 背景色（薄いグレー） */
}

/* テーブルの偶数行のスタイル（縞模様効果） */
.saved-orders-table tbody tr:nth-child(even) {
  background-color: #f9f9f9;        /* 背景色（非常に薄いグレー） */
}

/* テーブル行のホバー時のスタイル */
.saved-orders-table tbody tr.order-row:hover {
  background-color: #e9f5ff;        /* 背景色（薄い青） */
  cursor: pointer;                  /* ポインターカーソル */
}

/* 確定済み注文のスタイル */
.order-confirmed {
  background-color: #f0f0f0;        /* 背景色（グレー） */
  color: #777;                      /* 文字色（薄いグレー） */
}
/* 確定済み注文のホバー時のスタイル */
.order-confirmed:hover {
  background-color: #e0e0e0 !important; /* より濃いグレー（!importantで優先） */
}

/* 確定列のスタイル */
.col-confirm {
  width: 60px;                      /* 幅を60pxに固定 */
  text-align: center !important;    /* 文字を中央揃え */
}

/* 確定マーク（チェックマーク）のスタイル */
.confirmed-mark {
  font-weight: bold;                /* 文字を太字に */
  color: #28a745;                   /* 文字色（緑） */
  font-size: 20px;                  /* 文字サイズを大きく */
}
</style>