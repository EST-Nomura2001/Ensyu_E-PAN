<!-- 
  発注書作成フォームのVue.jsコンポーネント
  このコンポーネントは発注書を作成・編集・印刷する機能を持っています
-->
<template>
    <div id="purchase-order-page">
      <div id="purchase-order-container">
        <!-- 発注書ヘッダー部分 -->
        <header class="po-header">
          <h1 class="po-title">発注書</h1>
          
          <!-- 発注元の会社情報 -->
          <div class="company-info">
            <!-- v-modelは双方向データバインディング：入力内容がデータと同期される -->
            <input type="text" v-model="orderData.companyCd" class="issuer-company-name" placeholder="会社CD">
            <div class="issuer-address">
                <span>〒</span>
                <!-- 郵便番号入力欄 -->
                <input type="text" v-model="orderData.postalCode" class="issuer-postal-code" placeholder="郵便番号">
            </div>
            <!-- 住所入力欄（2行） -->
            <input type="text" v-model="orderData.address1" class="issuer-address-line" placeholder="住所1">
            <input type="text" v-model="orderData.address2" class="issuer-address-line" placeholder="住所2">
            
            <!-- 連絡先情報（電話、FAX、メール、担当者） -->
            <div class="issuer-contact-row">
                <label>TEL:</label>
                <input type="text" v-model="orderData.tel" placeholder="電話番号">
            </div>
            <div class="issuer-contact-row">
                <label>FAX:</label>
                <input type="text" v-model="orderData.fax" placeholder="FAX番号">
            </div>
            <div class="issuer-contact-row">
                <label>Mail:</label>
                <input type="email" v-model="orderData.email" placeholder="メールアドレス">
            </div>
            <div class="issuer-contact-row">
                <label>担当者:</label>
                <input type="text" v-model="orderData.manager" placeholder="担当者">
            </div>
          </div>
  
          <!-- 社印表示エリア -->
          <div class="company-stamp">
            社印
          </div>
        </header>
  
        <!-- 宛先情報 -->
        <section class="po-customer">
          <div class="customer-name-wrapper">
            <!-- 発注先の会社名入力 -->
            <input type="text" v-model="orderData.customerName" class="customer-name-input" placeholder="宛名を入力">
            <span class="honorific">御中</span>
          </div>
        </section>
  
        <!-- 発注日・番号・消費税率の情報 -->
        <section class="po-meta-info">
            <div class="tax-rate">
                <!-- .numberは数値として扱うためのVue.jsの修飾子 -->
                <p><strong>消費税率 <input type="number" v-model.number="orderData.tax" class="tax-rate-input"> %</strong></p>
            </div>
            <div class="order-number">
                <span>発注番号</span>
                <input type="text" v-model="orderData.quotation" class="meta-input" placeholder="見積番号">
            </div>
            <div class="order-date">
                <span>発注日</span>
                <!-- type="date"で日付選択機能付きの入力欄 -->
                <input type="date" v-model="orderData.orderDate" class="meta-input" placeholder="発注日">
            </div>
            <div class="delivery-date">
                <span>納入日</span>
                <input type="date" v-model="orderData.deliveryDate" class="meta-input" placeholder="納入日">
            </div>
            <div class="payment-date">
                <span>支払日</span>
                <input type="date" v-model="orderData.paymentDate" class="meta-input" placeholder="支払日">
            </div>
            <div class="payment-terms">
                <span>支払条件</span>
                <input type="text" v-model="orderData.paymentTerms" class="meta-input" placeholder="支払条件">
            </div>
        </section>
  
        <!-- 発注詳細情報 -->
        <section class="po-details">
          <p>下記のとおり、発注いたします。</p>
          <table class="details-table">
            <tbody>
              <!-- 各詳細項目の入力欄 -->
              <tr>
                <th>件名:</th>
                <td><input type="text" v-model="orderData.title" placeholder="件名"></td>
              </tr>
              <tr>
                <th>現場住所:</th>
                <td><input type="text" v-model="orderData.location"></td>
              </tr>
              <tr>
                <th>工期:</th>
                <td><input type="text" v-model="orderData.period"></td>
              </tr>
              <tr>
                <th>見積No:</th>
                <td><input type="text" v-model="orderData.quotation" placeholder="見積番号"></td>
              </tr>
            </tbody>
          </table>
        </section>
  
        <!-- 商品リスト -->
        <section class="po-items">
          <!-- datalistは入力候補を表示するHTMLの機能 -->
          <datalist id="product-list">
            <!-- v-forでpredefinedProducts配列の各要素を繰り返し表示 -->
            <!-- :keyは各要素を識別するためのユニークな値 -->
            <option v-for="product in predefinedProducts" :key="product" :value="product"></option>
          </datalist>
  
          <table class="items-table">
            <thead>
              <tr>
                <th class="col-no">No</th>
                <th class="col-product">商品</th>
                <th class="col-quantity">数量</th>
                <!-- no-printクラスは印刷時に非表示にするためのクラス -->
                <th class="col-actions no-print"></th>
              </tr>
            </thead>
            <tbody>
              <!-- v-forで商品項目を繰り返し表示 -->
              <!-- (item, index)でitemは商品データ、indexは配列の番号 -->
              <tr v-for="(item, index) in orderData.orderItems" :key="index">
                <td>{{ index + 1 }}</td>
                <td>
                  <input type="text" v-model="item.itemCd" list="product-list" placeholder="商品コードまたは商品名">
                </td>
                <td>
                  <input type="number" v-model.number="item.amount" placeholder="数量">
                </td>
                <td class="no-print">
                  <button @click="removeItem(index)" class="remove-btn">削除</button>
                </td>
              </tr>
            </tbody>
          </table>
          <!-- 新しい商品行を追加するボタン -->
          <button @click="addItem" class="add-btn no-print">＋ 行を追加</button>
        </section>
  
        <!-- 備考欄 -->
        <section class="po-notes">
          <label for="notes">備考:</label>
          <!-- textareaは複数行のテキスト入力欄 -->
          <textarea id="notes" v-model="orderData.other" rows="4"></textarea>
        </section>
      </div>
  
      <!-- フッター部分のボタン群 -->
      <footer class="actions-footer no-print">
        <!-- 新規作成ボタン -->
        <button @click="resetForm" class="action-btn clear-btn">新規作成フォーム</button>
        
        <!-- 下書き保存ボタン -->
        <button @click="handleSave(false)" class="action-btn save-btn" :disabled="isSaving">
          {{ isSaving ? '保存中...' : (orderData.id ? '下書きを更新' : '下書き保存') }}
        </button>
        
        <!-- 確定保存ボタン -->
        <button @click="handleSave(true)" class="action-btn save-btn" :disabled="isSaving">
          {{ isSaving ? '保存中...' : (orderData.id ? '確定内容を更新' : '確定保存') }}
        </button>
        
        <!-- 印刷ボタン -->
        <button @click="printPage" class="action-btn print-btn">印刷</button>
      </footer>
  
      <!-- 確定済みの場合の警告バナー -->
      <!-- v-ifは条件に合致する場合のみ要素を表示 -->
      <div v-if="orderData.confirmFlg" class="confirmed-banner no-print">
        <p>この発注書は確定済みです。内容は変更できません。</p>
      </div>
    </div>
  </template>
  
  <script>
  // Axiosライブラリをインポート（HTTP通信を行うためのライブラリ）
  import axios from 'axios';
  
  // APIサーバーのベースURL
  const API_BASE_URL = 'http://localhost:3000';
  
  // Vue.jsコンポーネントの定義
  export default {
    name: 'PurchaseOrder', // コンポーネントの名前
    props: ['id'], // 親コンポーネントから受け取るプロパティ（発注書のID）
    
    // データ定義：このコンポーネントで管理する変数
    data() {
      return {
        isSaving: false, // 保存中かどうかを管理するフラグ
        
        // 商品名の候補リスト
        predefinedProducts: [
          'めん', 'スープ', '醤油', '味噌', '豚骨', '昆布', 'チャーシュー', 'メンマ', 'のり', 'ネギ', '卵', 'ラード', 'ごま油', 'ニンニク', 'ラー油', 'スパイス', 'お酢'
        ],
        
        // 発注書のデータ（resetForm or loadOrderで初期化される）
        orderData: {
          id: null,                // ID
          title: '',               // 件名
          quotation: '',           // 見積番号
          tax: 10,                 // 税率
          orderDate: '',           // 発注日
          deliveryDate: '',        // 納入日
          paymentDate: '',         // 支払日
          paymentTerms: '',        // 支払条件
          confirmFlg: false,       // 発注フラグ
          companyCd: '',           // 発注先会社CD
          manager: '',             // 担当者
          storeCd: '',             // 納入先事業所CD
          other: '',               // 備考
          orderItems: [
            {
              id: null,                // ID
              pOrderListId: null,      // 発注書CD（親ID）
              itemCd: '',              // 商品コード
              amount: 1                // 数量
            }
          ]
        },
      };
    },
    
    // watchは特定の値の変化を監視する機能
    watch: {
      // URLのクエリパラメータ（?id=xxx）が変更されたときの処理
      '$route.query.id': {
        immediate: true, // コンポーネント作成時にも実行
        handler(newId) {
          if (newId) {
            // IDがある場合は既存データを読み込み
            this.loadOrder(newId);
          } else {
            // IDがない場合は新規作成フォームを表示
            this.resetForm();
          }
        }
      }
    },
    
    // メソッド定義：このコンポーネントで使用する関数
    methods: {
      // 新規作成用のフォーム初期化
      resetForm() {
        // URLを新規作成用に変更
        this.$router.push({ name: 'PurchaseOrder' });
        
        // フォームを初期値でリセット
        this.orderData = {
          id: null,
          title: '',
          quotation: '',
          tax: 10,
          orderDate: '',
          deliveryDate: '',
          paymentDate: '',
          paymentTerms: '',
          confirmFlg: false,
          companyCd: '',   // 会社CD
          manager: '',
          storeCd: '',     // 事業所CD
          other: '',
          orderItems: [
            {
              id: null,                // ID
              pOrderListId: null,      // 発注書CD（親ID）
              itemCd: '',              // 商品コード
              amount: 1                // 数量
            }
          ],
        };
      },
      
      // 既存データの読み込み（非同期処理）
      async loadOrder(orderId) {
        try {
          const response = await axios.get(`http://localhost:5000/api/PurchaseOrders/${orderId}`);
          if (response.data && response.data.success) {
            this.orderData = response.data.data;
          } else {
            alert('発注書が見つかりません');
            this.resetForm();
          }
        } catch (error) {
          alert('発注書の取得に失敗しました');
          this.resetForm();
        }
      },
  
      // 商品行を追加する関数
      addItem() {
        // 確定済みの場合は追加不可
        if (this.orderData.confirmFlg) return;
        
        // 新しい商品行を配列に追加
        this.orderData.orderItems.push({ id: null, pOrderListId: null, itemCd: '', amount: 1 });
      },
  
      // 商品行を削除する関数
      removeItem(index) {
        // 確定済みの場合は削除不可
        if (this.orderData.confirmFlg) return;
        
        if (this.orderData.orderItems.length > 1) {
          // 2行以上ある場合は指定した行を削除
          this.orderData.orderItems.splice(index, 1);
        } else {
          // 1行しかない場合は削除不可
          alert('最低1行は必要です。');
        }
      },
  
      // 保存ボタンが押されたときの処理
      handleSave(isConfirm) {
        if (this.orderData.confirmFlg) {
          alert('この発注書は確定済みのため、保存できません。');
          return;
        }
        this.orderData.confirmFlg = isConfirm;
        this.saveOrder();
      },
  
      // 実際の保存処理（非同期処理）
      async saveOrder() {
        this.isSaving = true;
        try {
          // 新規作成用DTO
          const payload = {
            id: this.orderData.id,
            title: this.orderData.title,
            quotation: this.orderData.quotation,
            tax: this.orderData.tax,
            orderDate: this.orderData.orderDate,
            deliveryDate: this.orderData.deliveryDate,
            paymentDate: this.orderData.paymentDate,
            paymentTerms: this.orderData.paymentTerms,
            confirmFlg: this.orderData.confirmFlg,
            companyCd: this.orderData.companyCd,
            manager: this.orderData.manager,
            storeCd: this.orderData.storeCd,
            other: this.orderData.other,
            orderItems: this.orderData.orderItems.map(item => ({
              id: item.id,
              pOrderListId: item.pOrderListId,
              itemCd: item.itemCd,
              amount: item.amount
            }))
          };
          const response = await axios.post('http://localhost:5000/api/PurchaseOrders', payload);
          if (response.data && response.data.success) {
            alert('発注書が正常に保存されました。');
            this.$router.push({ name: 'SavedOrders' });
          } else {
            alert('保存に失敗しました');
          }
        } catch (error) {
          alert('保存時にエラーが発生しました');
        } finally {
          this.isSaving = false;
        }
      },
      
      // 印刷機能
      printPage() {
        // ブラウザの印刷機能を呼び出し
        window.print();
      },

      async updateOrder() {
        this.isSaving = true;
        try {
          const payload = {
            id: this.orderData.id,
            title: this.orderData.title,
            quotation: this.orderData.quotation,
            tax: this.orderData.tax,
            orderDate: this.orderData.orderDate,
            deliveryDate: this.orderData.deliveryDate,
            paymentDate: this.orderData.paymentDate,
            paymentTerms: this.orderData.paymentTerms,
            confirmFlg: this.orderData.confirmFlg,
            companyCd: this.orderData.companyCd,
            manager: this.orderData.manager,
            storeCd: this.orderData.storeCd,
            other: this.orderData.other,
            orderItems: this.orderData.orderItems.map(item => ({
              id: item.id,
              pOrderListId: item.pOrderListId,
              itemCd: item.itemCd,
              amount: item.amount
            }))
          };
          const response = await axios.put(`http://localhost:5000/api/PurchaseOrders/${this.orderData.id}`, payload);
          if (response.data && response.data.success) {
            alert('発注書が正常に更新されました。');
            this.orderData = response.data.data;
          } else {
            alert('更新に失敗しました');
          }
        } catch (error) {
          alert('更新時にエラーが発生しました');
        } finally {
          this.isSaving = false;
        }
      },

      async addOrderItem(item) {
        const payload = {
          itemCd: item.itemCd,
          amount: item.amount
        };
        await axios.post(`http://localhost:5000/api/PurchaseOrders/${this.orderData.id}/items`, payload);
      },
    }
  };
  </script>
  
  <!-- 
    スタイル部分：このコンポーネント専用のCSS
    scopedが付いているので、他のコンポーネントには影響しない
  -->
  <style scoped>
  /* メインページのスタイル */
  #purchase-order-page {
    font-family: 'Meiryo', 'メイリオ', 'Hiragino Kaku Gothic ProN', 'ヒラギノ角ゴ ProN W3', sans-serif;
    background-color: #f4f4f4; /* 薄いグレーの背景 */
    padding: 20px;
  }
  
  /* 発注書本体のコンテナ */
  #purchase-order-container {
    background-color: #fff; /* 白い背景 */
    width: 800px; /* 固定幅 */
    padding: 40px;
    margin: 0 auto; /* 中央寄せ */
    font-size: 14px;
    box-shadow: 0 4px 12px rgba(0,0,0,0.05); /* 影をつけて立体感を演出 */
    border: 1px solid #e0e0e0; /* 薄いグレーの境界線 */
    border-radius: 8px; /* 角を丸める */
  }
  
  /* ヘッダー部分のスタイル */
  .po-header {
    position: relative; /* 子要素を絶対位置で配置するため */
    display: flex;
    justify-content: flex-end; /* 右寄せ */
    align-items: flex-start; /* 上揃え */
    border-bottom: 2px solid #000; /* 下線 */
    padding-bottom: 20px;
    margin-bottom: 20px;
  }
  
  /* 「発注書」タイトル */
  .po-title {
    font-size: 28px;
    font-weight: bold;
    margin: 0;
    position: absolute; /* 絶対位置 */
    left: 50%; /* 左から50%の位置 */
    top: 0;
    transform: translateX(-50%); /* 中央寄せ */
  }
  
  /* 会社情報のスタイル */
  .company-info {
    display: flex;
    flex-direction: column; /* 縦並び */
    align-items: flex-end; /* 右寄せ */
    gap: 2px; /* 要素間の間隔 */
    font-size: 12px;
    margin-right: 100px; /* 社印のスペースを確保 */
  }
  
  /* 会社情報の入力欄 */
  .company-info input {
    border: 1px solid #fff; /* 通常時は境界線なし */
    background-color: transparent; /* 透明な背景 */
    text-align: right; /* 右寄せ */
    padding: 2px 4px;
    font-size: 12px;
    font-family: inherit; /* 親要素のフォントを継承 */
    width: 180px;
    border-radius: 3px;
    transition: border-color 0.3s; /* 境界線の色変化にアニメーション */
  }
  
  /* 入力欄がフォーカスされたときのスタイル */
  .company-info input:focus { 
    outline: none; /* デフォルトのアウトラインを無効化 */
    border: 1px solid #007bff; /* 青い境界線 */
    background-color: #f0f8ff; /* 薄い青の背景 */
  }
  
  /* 各入力欄の個別スタイル */
  .issuer-company-name { font-size: 14px !important; font-weight: bold; margin-bottom: 5px; }
  .issuer-address { display: flex; justify-content: flex-end; align-items: center; }
  .issuer-postal-code { width: 70px !important; }
  .issuer-address-line { width: 220px !important; }
  .issuer-contact-row { display: flex; justify-content: flex-end; align-items: center; }
  .issuer-contact-row label { white-space: nowrap; margin-right: 5px; }
  
  /* 社印のスタイル */
  .company-stamp {
    position: absolute;
    right: 0;
    top: 0;
    border: 3px solid red; /* 赤い境界線 */
    color: red;
    font-size: 24px;
    font-weight: bold;
    width: 80px;
    height: 80px;
    display: flex;
    justify-content: center;
    align-items: center;
  }
  
  /* 宛先情報のスタイル */
  .po-customer { margin-bottom: 20px; }
  .customer-name-wrapper { display: flex; align-items: flex-end; border-bottom: 1px solid #000; width: 50%; }
  .customer-name-input { border: none; font-size: 18px; font-weight: bold; flex-grow: 1; padding: 5px; }
  .customer-name-input:focus { outline: none; background-color: #f0f8ff; }
  .honorific { font-size: 16px; margin-left: 10px; }
  
  /* メタ情報（発注日、番号等）のスタイル */
  .po-meta-info {
    display: flex;
    justify-content: flex-end;
    align-items: center;
    gap: 20px;
    margin-bottom: 20px;
    text-align: right;
  }
  .tax-rate { margin-right: auto; text-align: left; display: flex; align-items: center; gap: 5px;}
  .tax-rate-input {
    border: 1px solid #ccc;
    padding: 2px 4px;
    border-radius: 4px;
    width: 50px;
    text-align: right;
  }
  .order-number, .order-date { display: flex; align-items: center; gap: 8px; }
  .meta-input {
    border: 1px solid #ccc;
    padding: 4px 8px;
    border-radius: 4px;
    font-size: 14px;
  }
  .meta-input:focus { outline: 1px solid #007bff; }
  
  /* 発注詳細テーブルのスタイル */
  .po-details { margin-bottom: 30px; }
  .details-table { width: 100%; border-collapse: collapse; }
  .details-table th, .details-table td { padding: 8px; }
  .details-table th { text-align: left; width: 100px; white-space: nowrap; background-color: #f8f9fa; }
  .details-table input {
    width: 100%;
    border: none;
    border-bottom: 1px solid #ccc;
    padding: 5px;
    font-size: 14px;
  }
  .details-table input:focus { outline: none; border-bottom-color: #007bff; background-color: #f0f8ff; }
  
  /* 商品テーブルのスタイル */
  .items-table { width: 100%; border-collapse: collapse; margin-bottom: 10px; table-layout: fixed; }
  .items-table th, .items-table td { border: 1px solid #000; padding: 10px; text-align: center; }
  .items-table thead { background-color: #e9ecef; }
  .items-table .col-no { width: 8%; }
  .items-table .col-product { width: auto; }
  .items-table .col-quantity { width: 15%; }
  .items-table .col-actions { width: 15%; }
  .items-table input {
    width: 100%;
    border: none;
    padding: 10px;
    text-align: center;
    box-sizing: border-box;
    background: transparent;
  }
  .items-table input[type="text"] { text-align: left; padding-left: 10px; }
  .items-table input:focus { outline: 1px solid #007bff; background-color: #f0f8ff; }
  .items-table td { position: relative; }
  
  /* 削除ボタンのスタイル */
  .items-table .remove-btn {
    border: none; background: #ff4d4d; color: white; border-radius: 50%;
    width: 25px; height: 25px; line-height: 25px; padding: 0;
    cursor: pointer; transition: background 0.3s;
  }
  .items-table .remove-btn:hover { background: #e60000; }
  
  /* 行追加ボタンのスタイル */
  .add-btn {
    padding: 8px 15px; border: 1px solid #ccc; border-radius: 4px;
    cursor: pointer; background-color: #28a745; color: white;
    font-size: 13px; font-weight: bold; transition: background 0.3s;
  }
  .add-btn:hover { background-color: #218838; }
  
  /* フッターのアクションボタン群 */
  .actions-footer {
    width: 880px; margin: 20px auto 0;
    display: flex; justify-content: center; gap: 20px;
  }
  .action-btn {
    padding: 12px 25px; border: none; border-radius: 5px;
    cursor: pointer; font-size: 16px; font-weight: bold;
    color: white; transition: background-color 0.3s, box-shadow 0.3s;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  }
  .action-btn:disabled { background-color: #ccc; cursor: not-allowed; }
  .clear-btn { background-color: #6c757d; }
  .clear-btn:hover:not(:disabled) { background-color: #5a6268; }
  .save-btn { background-color: #007bff; }
  .save-btn:hover:not(:disabled) { background-color: #0056b3; }
  .print-btn { background-color: #17a2b8; }
  .print-btn:hover:not(:disabled) { background-color: #138496; }
  
  /* 備考欄のスタイル */
  .po-notes { margin-top: 30px; }
  .po-notes textarea {
    width: 100%; border: 1px solid #ccc; border-radius: 4px;
    padding: 10px; font-size: 14px; margin-top: 5px;
  }
  
  /* 確定済みバナーのスタイル */
  .confirmed-banner {
    background-color: #fff3cd;
    color: #856404;
    border: 1px solid #ffeeba;
    border-radius: 5px;
    padding: 15px;
    margin: 20px auto 0;
    width: 800px;
    text-align: center;
    font-weight: bold;
  }
  
  /* 印刷時に非表示にするクラス */
  @media print {
    .no-print {
      display: none !important;
    }
  }
  </style>