package testtracks.pages.restfulbooker.admin;

import com.microsoft.playwright.Locator;
import com.microsoft.playwright.Page;
import testtracks.configuration.TestSettings;
import testtracks.ui.common.BasePage;

public final class RoomsPage extends BasePage {
  public Locator roomNamed(String roomName) {
    return page.getByText(roomName, new Page.GetByTextOptions().setExact(true));
  }

  public RoomsPage(Page page, TestSettings settings) {
    super(page, settings);
  }
}
