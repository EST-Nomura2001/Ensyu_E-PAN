<!-- 
  発注書作成フォームのVue.jsコンポーネント
  このコンポーネントは発注書を作成・編集・印刷する機能を持っています
-->
<template>

  <CommonHeader />
  <div id="purchase-order-page">
    <div id="purchase-order-container">
      <!-- 発注書ヘッダー部分 -->
      <header class="po-header">
        <h1 class="po-title">発注書</h1>
        
        <!-- 発注元の会社情報 -->
        <div class="company-info">
          <!-- v-modelは双方向データバインディング：入力内容がデータと同期される -->
          <input type="text" v-model="purchaseOrder.Company.Id" class="issuer-company-name" placeholder="会社CD" @change="onCompanyChange">
          <div class="issuer-address">
              <span>〒</span>
              <!-- 郵便番号入力欄 -->
              <input type="text" v-model="purchaseOrder.Postal_Code" class="issuer-postal-code" placeholder="郵便番号" readonly>
          </div>
          <!-- 住所入力欄（2行） -->
          <input type="text" v-model="purchaseOrder.Address1" class="issuer-address-line" placeholder="住所1" readonly>
          <input type="text" v-model="purchaseOrder.Address2" class="issuer-address-line" placeholder="住所2" readonly>
          
          <!-- 連絡先情報（電話、FAX、メール、担当者） -->
          <div class="issuer-contact-row">
              <label>TEL:</label>
              <input type="text" v-model="purchaseOrder.Tel" placeholder="電話番号" readonly>
          </div>
          <div class="issuer-contact-row">
              <label>FAX:</label>
              <input type="text" v-model="purchaseOrder.Fax" placeholder="FAX番号" readonly>
          </div>
          <div class="issuer-contact-row">
              <label>Mail:</label>
              <input type="email" v-model="purchaseOrder.Email" placeholder="メールアドレス" readonly>
          </div>
          <div class="issuer-contact-row">
              <label>担当者:</label>
              <input type="text" v-model="purchaseOrder.Manager" placeholder="担当者">
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
          <input type="text" v-model="purchaseOrder.CustomerName" class="customer-name-input" placeholder="宛名を入力" readonly>
          <span class="honorific">御中</span>
        </div>
      </section>

      <!-- 消費税率と発注番号・発注日の情報 -->
      <section class="po-meta-info">
        <!-- 上段：消費税率（左）と発注番号・発注日（右） -->
        <div class="meta-top-row">
          <div class="tax-rate">
            <span>消費税率</span>
            <input type="number" v-model.number="purchaseOrder.Tax" class="tax-rate-input" min="0" max="100">
            <span>%</span>
          </div>
          <div class="order-info">
            <div class="meta-item">
              <span>発注番号</span>
              <input type="text" v-model="purchaseOrder.Quotation" class="meta-input">
            </div>
            <div class="meta-item">
              <span>発注日</span>
              <input type="date" v-model="purchaseOrder.Order_Date" class="meta-input">
            </div>
          </div>
        </div>
      </section>

      <p class="order-statement">下記のとおり、発注いたします。</p>

      <!-- 発注詳細情報 -->
      <section class="po-details">
        <table class="details-table">
          <tbody>
            <!-- 各詳細項目の入力欄 -->
            <tr>
              <th>件名:</th>
              <td><input type="text" v-model="purchaseOrder.Title" placeholder="件名"></td>
            </tr>
            <tr>
              <th>現場住所:</th>
              <td>
                <select v-model="purchaseOrder.Store.Id" @change="onLocationChange">
                  <option v-for="store in stores" :key="store.id" :value="store.id">
                    {{ store.address1 }}{{ store.address2 }}
                  </option>
                </select>
              </td>
            </tr>
            <tr>
              <th>工期:</th>
              <td><input type="date" v-model="purchaseOrder.Delivery_Date"></td>
            </tr>
            <tr>
              <th>支払期日:</th>
              <td><input type="date" v-model="purchaseOrder.Payment_Date"></td>
            </tr>
            <tr>
              <th>支払条件:</th>
              <td><input type="text" v-model="purchaseOrder.Payment_Terms" placeholder="月末締め、翌月末払い"></td>
            </tr>
            <tr>
              <th>見積No:</th>
              <td><input type="text" v-model="purchaseOrder.Quotation" placeholder="見積番号"></td>
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
            <tr v-for="(item, index) in purchaseOrder.OrderItems" :key="index">
              <td>{{ index + 1 }}</td>
              <td style="vertical-align: top;">
                <input
                  type="text"
                  v-model="item.Item_Name"
                  list="product-list"
                  placeholder="商品コードまたは商品名"
                  @change="onItemNameChange(item)"
                  style="margin-bottom:2px;"
                >
                <div v-if="item.Item_Cd === 0 && item.Item_Name === 'その他' && item.Other_ItemName && item.Other_ItemName.trim() !== ''" style="margin-top:2px; font-size:15px;">
                  {{ item.Other_ItemName }}
                </div>
              </td>
              <td>
                <input type="number" v-model.number="item.Amount" placeholder="数量">
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
        <textarea id="notes" v-model="purchaseOrder.Other" rows="4"></textarea>
      </section>
    </div>

    <!-- フッター部分のボタン群 -->
    <footer class="actions-footer no-print">
      <!-- 新規作成ボタン -->
      <button @click="resetForm" class="action-btn clear-btn">新規作成フォーム</button>
      
      <!-- 下書き保存ボタン -->
      <button @click="handleSave(false)" class="action-btn save-btn" :disabled="isSaving">
        {{ isSaving ? '保存中...' : (purchaseOrder.Id ? '下書きを更新' : '下書き保存') }}
      </button>
      
      <!-- 確定保存ボタン -->
      <button @click="handleSave(true)" class="action-btn save-btn" :disabled="isSaving">
        {{ isSaving ? '保存中...' : (purchaseOrder.Id ? '確定内容を更新' : '確定保存') }}
      </button>
      
      <!-- 印刷ボタン -->
      <button @click="printPage" class="action-btn print-btn">印刷</button>
    </footer>

    <!-- 確定済みの場合の警告バナー -->
    <!-- v-ifは条件に合致する場合のみ要素を表示 -->
    <div v-if="purchaseOrder.Confirm_Flg" class="confirmed-banner no-print">
      <p>この発注書は確定済みです。内容は変更できません。</p>
    </div>
  </div>
</template>

<script>
// Axiosライブラリをインポート（HTTP通信を行うためのライブラリ）
import axios from 'axios';
import CommonHeader from '../components/CommonHeader.vue';

// APIサーバーのベースURL
const API_BASE_URL = 'http://localhost:5011';

// 5桁ランダム番号生成関数
function generateRandomQuotation() {
  return Math.floor(10000 + Math.random() * 90000);
}

// Vue.jsコンポーネントの定義
export default {
components:{
  CommonHeader
},


  name: 'PurchaseOrder', // コンポーネントの名前
  props: ['id'], // 親コンポーネントから受け取るプロパティ（発注書のID）
  
  // データ定義：このコンポーネントで管理する変数
  data() {
    const today = new Date();
    const yyyy = today.getFullYear();
    const mm = String(today.getMonth() + 1).padStart(2, '0');
    const dd = String(today.getDate()).padStart(2, '0');
    const todayStr = `${yyyy}-${mm}-${dd}`;
    return {
      isSaving: false,
      isUnmounted: false,
      predefinedProducts: [],
      predefinedProductsList: [], // idとitem_Name両方保持
      stores: [],
      purchaseOrder: {
        Id: null,
        Title: '',
        Quotation: generateRandomQuotation(),
        Tax: 10,
        Order_Date: todayStr,
        Delivery_Date: '',
        Payment_Date: '',
        Payment_Terms: '',
        Confirm_Flg: false,
        Company: { Id: '' },
        Store: { Id: 1 },
        Manager: '',
        Other: '',
        OrderItems: [
          { Id: null, Item_Cd: '', Item_Name: '', Amount: 1, Other_ItemName: '' }
        ],
        Postal_Code: '',
        Address1: '',
        Address2: '',
        Tel: '',
        Fax: '',
        Email: '',
        CustomerName: ''
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
    // 既存データの読み込み（非同期処理）
    async loadOrder(orderId) {
      try {
        const response = await axios.get(`${API_BASE_URL}/api/PurchaseOrders/${orderId}`);
        if (response.data && response.data.success) {
          const d = response.data.data;
          this.purchaseOrder = {
            Id: d.id,
            Title: d.title,
            Quotation: d.quotation,
            Tax: d.tax,
            Order_Date: d.order_Date ? d.order_Date.substring(0, 10) : '',
            Delivery_Date: d.delivery_Date ? d.delivery_Date.substring(0, 10) : '',
            Payment_Date: d.payment_Date ? d.payment_Date.substring(0, 10) : '',
            Payment_Terms: d.payment_Terms,
            Confirm_Flg: d.confirm_Flg,
            Manager: d.manager,
            Other: d.other,
            Company: d.company ? {
              Id: d.company.id,
              C_Name: d.company.c_Name,
              Address1: d.company.address1,
              Address2: d.company.address2,
              Post_Code: d.company.post_Code,
              Mail: d.company.mail,
              Tel: d.company.tel,
              Fax: d.company.fax
            } : { Id: '' },
            Store: d.store ? {
              Id: d.store.id,
              C_Name: d.store.c_Name,
              Address1: d.store.address1,
              Address2: d.store.address2,
              Post_Code: d.store.post_Code,
              Mail: d.store.mail,
              Tel: d.store.tel,
              Fax: d.store.fax
            } : { Id: 1 },
            OrderItems: Array.isArray(d.orderItems) ? d.orderItems.map(item => ({
              Id: item.id,
              Item_Cd: item.item_Cd,
              Item_Name: item.item_Name,
              Amount: item.amount,
              Other_ItemName: item.other_ItemName
            })) : [
              { Id: null, Item_Cd: '', Item_Name: '', Amount: 1, Other_ItemName: '' }
            ],
            Postal_Code: d.company?.post_Code || '',
            Address1: d.company?.address1 || '',
            Address2: d.company?.address2 || '',
            Tel: d.company?.tel || '',
            Fax: d.company?.fax || '',
            Email: d.company?.mail || '',
            CustomerName: d.company?.c_Name || ''
          };
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
      if (this.purchaseOrder.Confirm_Flg) return;
      this.purchaseOrder.OrderItems.push({ Id: null, Item_Cd: '', Item_Name: '', Amount: 1, Other_ItemName: '' });
    },

    // 商品行を削除する関数
    removeItem(index) {
      if (this.purchaseOrder.Confirm_Flg) return;
      if (this.purchaseOrder.OrderItems.length > 1) {
        this.purchaseOrder.OrderItems.splice(index, 1);
      } else {
        alert('最低1行は必要です。');
      }
    },

    // 保存ボタンが押されたときの処理
    async handleSave(isConfirm) {
      if (this.purchaseOrder.Confirm_Flg) {
        alert('この発注書は確定済みのため、保存できません。');
        return;
      }
      this.purchaseOrder.Confirm_Flg = isConfirm;
      await this.updateOrder();
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
          Id: this.purchaseOrder.Id,
          Title: this.purchaseOrder.Title || "タイトル未入力",
          Quotation: Number(this.purchaseOrder.Quotation),
          Tax: Number(this.purchaseOrder.Tax),
          Order_Date: this.purchaseOrder.Order_Date ? this.purchaseOrder.Order_Date + 'T00:00:00.000Z' : null,
          Delivery_Date: this.purchaseOrder.Delivery_Date ? this.purchaseOrder.Delivery_Date + 'T00:00:00.000Z' : null,
          Payment_Date: this.purchaseOrder.Payment_Date ? this.purchaseOrder.Payment_Date + 'T00:00:00.000Z' : null,
          Payment_Terms: this.purchaseOrder.Payment_Terms || "支払条件未入力",
          Confirm_Flg: this.purchaseOrder.Confirm_Flg,
          Manager: this.purchaseOrder.Manager || "担当者未入力",
          Other: this.purchaseOrder.Other || "備考なし",
          Company: this.purchaseOrder.Company && this.purchaseOrder.Company.Id ? {
            Id: Number(this.purchaseOrder.Company.Id),
            C_Name: this.purchaseOrder.CustomerName || "会社名未入力",
            Address1: this.purchaseOrder.Address1 || "住所未入力",
            Address2: this.purchaseOrder.Address2 || "",
            Post_Code: this.purchaseOrder.Postal_Code || "",
            Mail: this.purchaseOrder.Email || "",
            Tel: this.purchaseOrder.Tel || "",
            Fax: this.purchaseOrder.Fax || ""
          } : null,
          Store: this.purchaseOrder.Store && this.purchaseOrder.Store.Id ? {
            Id: Number(this.purchaseOrder.Store.Id),
            C_Name: this.purchaseOrder.Store.c_Name || "店舗名未入力",
            Address1: this.purchaseOrder.Store.address1 || "",
            Address2: this.purchaseOrder.Store.address2 || "",
            Post_Code: this.purchaseOrder.Store.post_Code || "",
            Mail: this.purchaseOrder.Store.mail || "",
            Tel: this.purchaseOrder.Store.tel || "",
            Fax: this.purchaseOrder.Store.fax || ""
          } : null,
          OrderItems: (this.purchaseOrder.OrderItems && this.purchaseOrder.OrderItems.length > 0)
            ? this.purchaseOrder.OrderItems
              .filter(item => item.Item_Name && item.Item_Name.trim() !== '')
              .map(item => ({
                Id: item.Id || 0,
                Item_Cd: Number(item.Item_Cd) || 0,
                Item_Name: item.Item_Name || "商品名未入力",
                Amount: Number(item.Amount),
                Other_ItemName: item.Other_ItemName || ""
              }))
            : null
        };
        // 送信前にリクエストボディを確認
        console.log(JSON.stringify(payload));
        // OrderItemsのバリデーション
        if (!payload.OrderItems || payload.OrderItems.length === 0) {
          alert('商品が1つも入力されていません。');
          this.isSaving = false;
          return;
        }
        // payloadをラップせずそのまま送信
        const response = await axios.put(`${API_BASE_URL}/api/PurchaseOrders/updateOrder/${this.purchaseOrder.Id}`, payload);
        if (response.data && response.data.success) {
          alert('発注書が正常に更新されました。');
          this.$router.push({ name: 'SavedOrders' });
          throw '__ROUTER_PUSH__';
        } else {
          alert('更新に失敗しました');
        }
      } catch (error) {
        if (error === '__ROUTER_PUSH__') return;
        if (!this.isUnmounted) alert('更新時にエラーが発生しました');
      } finally {
        this.isSaving = false;
      }
    },

    async onCompanyChange() {
      const id = this.purchaseOrder.Company.Id;
      if (!id) return;
      try {
        const res = await axios.get(`${API_BASE_URL}/api/Masters/company/${id}`);
        const c = res.data;
        this.purchaseOrder.Postal_Code = c.post_Code || '';
        this.purchaseOrder.Address1 = c.address1 || '';
        this.purchaseOrder.Address2 = c.address2 || '';
        this.purchaseOrder.Tel = c.tel || '';
        this.purchaseOrder.Fax = c.fax || '';
        this.purchaseOrder.Email = c.mail || '';
        this.purchaseOrder.CustomerName = c.c_Name || '';
      } catch (e) {
        alert('会社情報の取得に失敗しました');
      }
    },

    async fetchProducts() {
      try {
        const res = await axios.get(`${API_BASE_URL}/api/Masters/items`);
        if (Array.isArray(res.data)) {
          this.predefinedProducts = res.data.map(item => item.item_Name);
          this.predefinedProductsList = res.data; // idとitem_Name両方保持
        }
      } catch (e) {
        alert('商品リストの取得に失敗しました');
      }
    },

    async fetchStores() {
      try {
        const res = await axios.get(`${API_BASE_URL}/api/Masters/stores`);
        if (Array.isArray(res.data)) {
          this.stores = res.data;
        }
      } catch (e) {
        alert('店舗リストの取得に失敗しました');
      }
    },

    async genbajyuusyo() {
      try {
        const res = await axios.get(`${API_BASE_URL}/api/Masters/stores/`);
        if (Array.isArray(res.data)) {
          this.stores = res.data;
        }
      } catch (e) {
        alert('店舗リストの取得に失敗しました');
      }
    },

    onItemNameChange(item) {
      // 商品リストからitem_Nameに一致するidを探してセット
      const found = this.predefinedProductsList.find(p => p.item_Name === item.Item_Name);
      if (found) {
        item.Item_Cd = found.id;
        item.Other_ItemName = '';
      } else if (item.Item_Name === 'その他') {
        item.Item_Cd = 0;
        // 入力値はother_ItemNameに記録する（item_Nameは'その他'のまま）
        // 入力欄でother_ItemNameを編集できるようにする
      } else {
        item.Item_Cd = 0;
        item.Other_ItemName = item.Item_Name;
        item.Item_Name = 'その他';
      }
    },

    onLocationChange() {
      const selectedStore = this.stores.find(store => store.id === this.purchaseOrder.Store.Id);
      if (selectedStore) {
        this.purchaseOrder.Address1 = selectedStore.address1 || '';
        this.purchaseOrder.Address2 = selectedStore.address2 || '';
      }
    },
  },
  mounted() {
    this.fetchProducts();
    this.fetchStores();
  },
  beforeUnmount() {
    this.isUnmounted = true;
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
  margin-bottom: 20px;
}

.meta-top-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.tax-rate {
  display: flex;
  align-items: center;
  gap: 5px;
}

.tax-rate-input {
  border: 1px solid #ccc;
  padding: 2px 4px;
  border-radius: 4px;
  width: 50px;
  text-align: right;
}

.order-info {
  display: flex;
  gap: 20px;
  align-items: center;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 6px;
}

.meta-input {
  border: 1px solid #ccc;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 14px;
}
.meta-input:focus { outline: 1px solid #007bff; }

.order-statement {
  text-align: center;
  margin: 15px 0;
  font-weight: normal;
}

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

  .po-details p {
    text-align: center !important;
  }
  </style>