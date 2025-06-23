
  <style>
    body {
      font-family: sans-serif;
      padding: 20px;
    }
    table {
      border-collapse: collapse;
      width: 100%;
      table-layout: fixed;
    }
    th, td {
    
      border: 1px solid #ccc;
      padding: 6px;
      text-align: center;
      font-size: 12px;
    }
    th{
        width: 80px;
    }
    th.event-col {
      background: #f0f0f0;
      width: 120px;
    }
    td.empty {
      background-color: #f8f8f8;
      color: #aaa;
    }
    td.event-block {
      background-color: #d0f0c0;
      font-weight: bold;
    }
    input[type="time"] {
      width: 70px;
    }
  </style>

<template>
<h2>■Seaエリア 横時間軸タイムテーブル</h2>

<table id="timeTable">
  <thead>
    <tr>
      <th>イベント</th>
    </tr>
  </thead>
  <tbody>
    <!-- JavaScriptで生成 -->
  </tbody>
</table>
</template>

<script>
  const startHour = 9;
  const endHour = 23;
  const intervalMinutes = 30;

  const events = [
    { label: "たこ焼き", start: "10:00", end: "11:30" },
    { label: "綿あめ", start: "13:00", end: "14:00" },
    { label: "メロンソーダ", start: "15:00", end: "16:30" },
    { label: "鳩サブレ", start: "17:00", end: "17:30" },
  ];

  const table = document.getElementById("timeTable");
  const thead = table.querySelector("thead tr");
  const tbody = table.querySelector("tbody");

  const timeSlots = [];
  for (let h = startHour; h < endHour; h++) {
    for (let m = 0; m < 60; m += intervalMinutes) {
      timeSlots.push(`${String(h).padStart(2, "0")}:${String(m).padStart(2, "0")}`);
    }
  }

  // ヘッダーに時間を追加
  timeSlots.forEach(time => {
    const th = document.createElement("th");
    th.textContent = time;
    thead.appendChild(th);
  });

  function getMinutes(timeStr) {
    const [h, m] = timeStr.split(":").map(Number);
    return h * 60 + m;
  }

  function renderTable() {
    tbody.innerHTML = "";
    events.forEach((ev, idx) => {
      const tr = document.createElement("tr");

      // イベント名と時間入力欄
      const th = document.createElement("th");
      th.className = "event-col";
      th.innerHTML = `
        ${ev.label}<br>
        開始: <input type="time" value="${ev.start}" id="start${idx}"><br>
        終了: <input type="time" value="${ev.end}" id="end${idx}">
      `;
      tr.appendChild(th);

      const startMin = getMinutes(ev.start);
      const endMin = getMinutes(ev.end);

      const rowCols = [];
      let placed = false;

      timeSlots.forEach((time, tIdx) => {
        const tMin = getMinutes(time);

        if (tMin >= startMin && tMin < endMin && !placed) {
          // spanを計算
          let span = 0;
          for (let t = tIdx; t < timeSlots.length; t++) {
            const tCheck = getMinutes(timeSlots[t]);
            if (tCheck >= startMin && tCheck < endMin) {
              span++;
            }
          }

          const td = document.createElement("td");
          td.className = "event-block";
          td.colSpan = span;
          td.textContent = `［${ev.start}～${ev.end}］ ${ev.label}`;
          tr.appendChild(td);
          placed = true;
        } else if (tMin >= startMin && tMin < endMin) {
          // スキップ（colSpanで表示済み）
        } else {
          const td = document.createElement("td");
          td.className = "empty";
          tr.appendChild(td);
        }
      });

      tbody.appendChild(tr);
    });

    // イベント変更時に再描画
    events.forEach((ev, idx) => {
      const startInput = document.getElementById(`start${idx}`);
      const endInput = document.getElementById(`end${idx}`);

      startInput.addEventListener("input", () => {
        ev.start = startInput.value;
        renderTable();
      });
      endInput.addEventListener("input", () => {
        ev.end = endInput.value;
        renderTable();
      });
    });
  }

  renderTable();
</script>


