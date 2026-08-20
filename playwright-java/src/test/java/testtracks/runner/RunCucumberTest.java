package testtracks.runner;

import static io.cucumber.junit.platform.engine.Constants.GLUE_PROPERTY_NAME;

import org.junit.platform.suite.api.ConfigurationParameter;
import org.junit.platform.suite.api.IncludeEngines;
import org.junit.platform.suite.api.SelectPackages;
import org.junit.platform.suite.api.Suite;

@Suite
@IncludeEngines("cucumber")
@SelectPackages("testtracks.features")
@ConfigurationParameter(key = GLUE_PROPERTY_NAME, value = "testtracks")
public final class RunCucumberTest {}
