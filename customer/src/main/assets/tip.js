(function(){
  const style=document.createElement('style');
  style.textContent=`
    .tipBox{margin-top:14px;padding:13px;border:1px solid #dfe6ef;border-radius:13px;background:#f9fbfe}
    .tipTitle{font-size:13px;font-weight:800;color:#002b67;margin-bottom:9px}
    .tipOptions{display:grid;grid-template-columns:repeat(4,1fr);gap:7px}
    .tipOptions button{border:1px solid #d5deea;background:#fff;border-radius:10px;padding:10px 4px;font-weight:800;color:#002b67}
    .tipOptions button.selected{background:#002b67;color:#fff;border-color:#002b67}
    .tipCustom{display:none;margin-top:8px}
    .priceLine{display:flex;justify-content:space-between;gap:12px;margin:6px 0;font-size:13px}
    .priceLine.total{border-top:1px solid #e5cf77;padding-top:9px;margin-top:9px;font-size:17px;font-weight:900;color:#002b67}
  `;
  document.head.appendChild(style);

  function money(v){return 'R'+Number(v||0).toFixed(2).replace('.00','');}

  window.setTip=function(amount){
    draft.tip=Number(amount)||0;
    draft.total=Number(draft.price||0)+draft.tip;
    const custom=document.getElementById('customTipWrap');
    if(custom) custom.style.display='none';
    document.querySelectorAll('.tipOptions button').forEach(b=>b.classList.remove('selected'));
    const b=document.querySelector(`[data-tip="${amount}"]`);
    if(b)b.classList.add('selected');
    updateTipSummary();
  };

  window.showCustomTip=function(){
    document.querySelectorAll('.tipOptions button').forEach(b=>b.classList.remove('selected'));
    const b=document.getElementById('customTipButton');if(b)b.classList.add('selected');
    const wrap=document.getElementById('customTipWrap');if(wrap)wrap.style.display='block';
    const input=document.getElementById('customTip');if(input){input.focus();setCustomTip();}
  };

  window.setCustomTip=function(){
    const input=document.getElementById('customTip');
    let v=input?parseFloat(input.value):0;
    if(!isFinite(v)||v<0)v=0;
    draft.tip=Math.round(v*100)/100;
    draft.total=Number(draft.price||0)+draft.tip;
    updateTipSummary();
  };

  window.updateTipSummary=function(){
    const fee=document.getElementById('deliveryFee');
    const tip=document.getElementById('tipAmount');
    const total=document.getElementById('orderTotal');
    if(fee)fee.textContent=money(draft.price);
    if(tip)tip.textContent=money(draft.tip);
    if(total)total.textContent=money(draft.total);
  };

  window.reviewOrder=function(){
    let g=id=>document.getElementById(id).value.trim();
    let pickup=g('pickup'),dropoff=g('dropoff');
    if(!pickup||!dropoff){alert('Please enter pickup and drop-off addresses.');return;}
    draft={pickup,dropoff,sender:g('sender'),senderPhone:g('senderPhone'),receiver:g('receiver')||'Receiver',receiverPhone:g('receiverPhone'),size:document.getElementById('size').value,notes:g('notes'),payment:document.getElementById('payment').value};
    draft.price=estimate(draft.size);
    draft.tip=0;
    draft.total=draft.price;
    set(`${header('Review Order','newOrder()')}<div class="wrap"><div class="card"><h2>Delivery Summary</h2><div class="route"><b>📍 Pickup</b><br>${esc(draft.pickup)}</div><div class="route"><b>🏁 Drop-off</b><br>${esc(draft.dropoff)}</div><p><b>Receiver:</b> ${esc(draft.receiver)} ${draft.receiverPhone?'<br><span class="small">'+esc(draft.receiverPhone)+'</span>':''}</p><p><b>Package:</b> ${esc(draft.size)}</p><p><b>Payment:</b> ${esc(draft.payment)}</p><div class="tipBox"><div class="tipTitle">TIP YOUR DRIVER</div><div class="tipOptions"><button class="selected" data-tip="0" onclick="setTip(0)">No tip</button><button data-tip="10" onclick="setTip(10)">R10</button><button data-tip="20" onclick="setTip(20)">R20</button><button data-tip="30" onclick="setTip(30)">R30</button></div><button id="customTipButton" class="btn outline" style="margin-top:8px" onclick="showCustomTip()">Custom tip</button><div id="customTipWrap" class="tipCustom"><input class="field" id="customTip" type="number" min="0" step="1" placeholder="Enter tip amount" oninput="setCustomTip()"></div></div><div class="estimate"><div class="small">TEST ESTIMATE</div><div class="priceLine"><span>Delivery fee</span><b id="deliveryFee">${money(draft.price)}</b></div><div class="priceLine"><span>Driver tip</span><b id="tipAmount">R0</b></div><div class="priceLine total"><span>Total</span><span id="orderTotal">${money(draft.total)}</span></div><div class="small">The tip goes to the driver. Final production pricing will use real distance and delivery rules.</div></div><button class="btn gold" onclick="confirmOrder()">Confirm Order</button><button class="btn outline" onclick="newOrder()">Edit Order</button></div></div>${nav('new')}`);
  };

  window.orderDetails=function(id){
    current=orders.find(o=>o.id===id);if(!current)return ordersScreen();
    let idx=statuses.indexOf(current.status);
    let tip=Number(current.tip||0),total=Number(current.total||Number(current.price||0)+tip);
    set(`${header(current.id,'ordersScreen()')}<div class="wrap"><div class="card"><div class="row"><h2 style="margin:0">Delivery Status</h2><span class="status">${esc(current.status)}</span></div><div class="steps">${statuses.map((s,i)=>`<div class="step ${i<idx?'done':i===idx?'on':''}"><span class="dot"></span><div><b>${s}</b>${i===1&&current.driver?'<span class="small">Driver: '+esc(current.driver)+'</span>':''}</div></div>`).join('')}</div><div class="route"><b>📍 Pickup</b><br>${esc(current.pickup)}</div><div class="route"><b>🏁 Drop-off</b><br>${esc(current.dropoff)}</div><p><b>Package:</b> ${esc(current.size)}</p><div class="estimate"><div class="priceLine"><span>Delivery fee</span><b>${money(current.price)}</b></div><div class="priceLine"><span>Driver tip</span><b>${money(tip)}</b></div><div class="priceLine total"><span>Total</span><span>${money(total)}</span></div></div>${idx>=1&&idx<6?'<button class="btn outline" onclick="tripChat()">💬 Trip Chat</button>':''}${idx>=1&&current.driver?'<button class="btn blue" onclick="dial(\'0710000000\')">Call Driver</button>':''}${idx<6?'<button class="btn gold" onclick="nextStatus()">TEST: Simulate Next Status</button>':''}</div></div>${nav('orders')}`);
  };
})();
