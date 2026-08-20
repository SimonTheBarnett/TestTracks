package testtracks.ui.common;

import com.microsoft.playwright.Page;
import testtracks.configuration.TestSettings;

public abstract class BasePage {
  protected final Page page;
  protected final TestSettings settings;

  protected BasePage(Page page, TestSettings settings) {
    this.page = page;
    this.settings = settings;
  }

  public void open(String relativeRoute) {
    page.navigate(relativeRoute);
  }
}
