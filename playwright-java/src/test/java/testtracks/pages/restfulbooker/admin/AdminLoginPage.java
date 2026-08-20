package testtracks.pages.restfulbooker.admin;

import com.microsoft.playwright.Locator;
import com.microsoft.playwright.Page;
import com.microsoft.playwright.options.AriaRole;
import testtracks.configuration.TestSettings;
import testtracks.ui.common.BasePage;

public final class AdminLoginPage extends BasePage {
  private static final String ADMIN_ROUTE = "admin";
  private static final String USERNAME_LABEL = "Username";
  private static final String PASSWORD_LABEL = "Password";
  private static final String LOGIN_BUTTON_NAME = "Login";

  private Locator username() {
    return page.getByLabel(USERNAME_LABEL);
  }

  private Locator password() {
    return page.getByLabel(PASSWORD_LABEL);
  }

  private Locator loginButton() {
    return page.getByRole(AriaRole.BUTTON, new Page.GetByRoleOptions().setName(LOGIN_BUTTON_NAME));
  }

  public AdminLoginPage(Page page, TestSettings settings) {
    super(page, settings);
  }

  public void logIn(String username, String password) {
    open(ADMIN_ROUTE);
    username().fill(username);
    password().fill(password);
    loginButton().click();
  }
}
