package za.co.deutronomagroup.droplink;

import android.app.Activity;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.webkit.JavascriptInterface;
import android.webkit.ValueCallback;
import android.webkit.WebChromeClient;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.webkit.WebViewClient;

public class MainActivity extends Activity {
  private WebView webView;
  private ValueCallback<Uri[]> filePathCallback;
  private static final int FILE_CHOOSER_REQUEST = 1001;

  @Override protected void onCreate(Bundle savedInstanceState) {
    super.onCreate(savedInstanceState);
    webView = new WebView(this);
    setContentView(webView);
    WebSettings s = webView.getSettings();
    s.setJavaScriptEnabled(true);
    s.setDomStorageEnabled(true);
    s.setAllowFileAccess(true);
    s.setAllowContentAccess(true);
    webView.addJavascriptInterface(new AndroidBridge(), "Android");
    webView.setWebViewClient(new WebViewClient());
    webView.setWebChromeClient(new WebChromeClient() {
      @Override public boolean onShowFileChooser(WebView v, ValueCallback<Uri[]> cb, FileChooserParams fp) {
        if (filePathCallback != null) filePathCallback.onReceiveValue(null);
        filePathCallback = cb;
        try { startActivityForResult(fp.createIntent(), FILE_CHOOSER_REQUEST); return true; }
        catch (Exception e) { filePathCallback = null; return false; }
      }
    });
    webView.loadUrl("file:///android_asset/index.html");
  }

  public class AndroidBridge {
    @JavascriptInterface public void openMaps(String d) {
      try { startActivity(new Intent(Intent.ACTION_VIEW, Uri.parse("geo:0,0?q=" + Uri.encode(d)))); } catch (Exception ignored) {}
    }
    @JavascriptInterface public void dial(String p) {
      try { startActivity(new Intent(Intent.ACTION_DIAL, Uri.parse("tel:" + p))); } catch (Exception ignored) {}
    }
  }

  @Override protected void onActivityResult(int rc, int resultCode, Intent data) {
    super.onActivityResult(rc, resultCode, data);
    if (rc == FILE_CHOOSER_REQUEST && filePathCallback != null) {
      filePathCallback.onReceiveValue(WebChromeClient.FileChooserParams.parseResult(resultCode, data));
      filePathCallback = null;
    }
  }

  @Override public void onBackPressed() {
    if (webView.canGoBack()) webView.goBack(); else super.onBackPressed();
  }
}
