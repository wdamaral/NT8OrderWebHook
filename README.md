# **OrderWebHook for NinjaTrader 8**

**OrderWebHook** is a custom NinjaScript indicator designed to bridge the gap between NinjaTrader 8 executions and external automation platforms. It monitors specific accounts for trade executions and forwards the data instantaneously via HTTP Webhooks to configured endpoints (ATS and QuantLynk).

## **🚀 Features**

* **Real-Time Monitoring**: Hooks into OnExecutionUpdate to capture fills immediately.  
* **Multi-Provider Architecture**: capable of sending data to multiple services simultaneously (currently ATS and QuantLynk).  
* **Asynchronous Processing**: Uses a Producer/Consumer pattern (BlockingCollection) to ensure HTTP requests do not block the UI or Trading thread.  
* **Integrated UI**: Injects a custom WPF control panel directly into the NinjaTrader **Chart Trader** interface.  
  * Toggle services ON/OFF via buttons.  
  * Live scrollable event log within the chart.  
* **Smart Position Tracking**: Calculates net position logic to determine if a signal is a Entry (Buy/Sell) or an Exit/Flatten command.

## **📋 Prerequisites**

* NinjaTrader 8  
* An active internet connection  
* Target webhook URLs (ATS, or QuantLynk endpoints)

## **📦 Installation**

You can install OrderWebHook using one of the two methods below.

### **Method 1: Automatic Import (Recommended)**

This method installs the pre-compiled assembly directly.

1. Download the OrderWebHook.zip file.  
2. Open **NinjaTrader 8**.  
3. Go to **Tools \> Import \> NinjaScript Add-On...**  
4. Select the downloaded .zip file.  
5. Restart NinjaTrader if prompted.

### **Method 2: Manual Installation (Source Code)**

Use this method if you wish to view or modify the source code.

1. **Download Source**: Download the OrderWebHook folder containing the source files.  
2. **Place Script**: Move the entire OrderWebHook folder into your NinjaTrader custom indicators directory:  
   * Documents\\NinjaTrader 8\\bin\\Custom\\Indicators  
3. **Add Dependencies**: Download Newtonsoft.Json.dll and place it into your NinjaTrader custom bin directory:  
   * Documents\\NinjaTrader 8\\bin\\Custom  
4. **Compile**:  
   * Open NinjaTrader 8\.  
   * Go to **New \> NinjaScript Editor**.  
   * Press F5 to compile the indicators.

## **⚙️ Configuration**

Add the indicator to your chart like any standard indicator (Ctrl+I).

### **1\. ATS Settings**

* **Enable ATS**: Master switch for the ATS provider.  
* **Webhook URL**: The destination endpoint.  
* **User ID**: Your specific ATS user identifier.  
* **Spam Key**: Authentication/Security key for the payload.

### **2\. QuantLynk Settings**

* **Enable QuantLynk**: Master switch for the QuantLynk provider.  
* **Webhook URL**: The destination endpoint.  
* **QV User ID**: QuantLynk user identifier.  
* **Alert ID**: The specific alert ID to trigger.

### **3\. General Settings**

* **Account Name**: Select the specific account to monitor (e.g., Sim101, MyFundedAccount). The indicator will ignore executions from other accounts.

## **🖥️ Usage**
<img width="1001" height="799" alt="image" src="https://github.com/user-attachments/assets/74c55e7f-cbbc-4fb7-bd64-af2e009434b5" />
<img width="1303" height="764" alt="image" src="https://github.com/user-attachments/assets/e78346ec-1b8d-4b3a-a587-d758a0cf2220" />

Once applied to a chart, ensure **Chart Trader** is open. The indicator detects the Chart Trader panel and injects a custom control grid at the bottom.

1. **Status Buttons**:  
   * **Green**: The webhook service is active.  
   * **Red**: The webhook service is disabled.  
   * *Clicking these buttons toggles the service instantaneously without reloading the script.*  
2. **Log Box**:  
   * Displays successful HTTP status codes (e.g., ATS: 200 (120ms)).  
   * Displays errors in Salmon color.

## **📡 Payload Structures**

### **ATS Payload**
```
{  
  "user_id": "string",  
  "spam-key": "string",  
  "contract": "ESZ23", // Automatic Futures Code conversion  
  "quantity": 1,  
  "price": 4500.50,  
  "trade_type": "buy", // buy, sell, or exit  
  "strategy": 0  
}
```
### **QuantLynk Payload**

**For Entries:**
```
{  
  "qv_user_id": "string",  
  "alert_id": "string",  
  "quantity": 1,  
  "order_type": "market",  
  "action": "buy" // or sell  
}
```

**For Exits/Flatten:**
```
{  
  "qv_user_id": "string",  
  "alert_id": "string",  
  "flatten": true  
}
```
## **🏗️ Technical Architecture**

* **WebhookOrchestrator**: Manages a background Task that consumes execution snapshots from a thread-safe queue. This ensures that network lag never affects the NinjaTrader core loop.  
* **ChartUiManager**: Uses System.Windows.Automation and VisualTreeHelper to find the specific WPF visual elements of the Chart Trader and dynamically injects the control grid.  
* **PositionTracker**: Maintains an internal state of the account's position for the specific instrument to determine if an execution is opening a new position or closing an existing one.

## **⚠️ Disclaimer**

**This is a third-party community tool.**

This software is **NOT** an official product of **NinjaTrader** or **QuantVue**. It is an independent open-source project created by the community.

This software is for educational purposes only. Do not risk money you cannot afford to lose. The authors allow this code to be used "as-is" and accept no liability for financial losses, missed webhooks, or technical failures.
