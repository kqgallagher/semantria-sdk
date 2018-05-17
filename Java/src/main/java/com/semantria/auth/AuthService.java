package com.semantria.auth;

import com.google.common.base.Strings;
import com.google.gson.Gson;
import com.semantria.utils.AuthRequest;
import com.semantria.utils.Utils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.File;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.StandardOpenOption;
import java.util.HashMap;
import java.util.Map;

public class AuthService {

    private static Logger log = LoggerFactory.getLogger(AuthService.class);

    private String authUrl = "https://semantria.com/auth";
    private static final String appKey = "cd954253-acaf-4dfa-a417-0a8cfb701f12";

    private String key;
    private String secret;
    private File cookieDir = null;
    private String cookieFileName = "semantria-session.dat";

    public AuthService() {
    }

    public AuthService withAuthUrl(String value) {
        authUrl = value;
        return this;
    }

    public String getKey() {
        return key;
    }

    public String getSecret() {
        return secret;
    }

    /**
     * Attempt to authenticate using username/password. Does not consume an auth session.
     * @throws CredentialException if username/password are invalid
     */
    public void authenticate(String username, String password) throws CredentialException {
        String requestData = getRequestData(username, password);
        String url = authUrl + "/session.json?appkey=" + appKey;
        AuthRequest req = AuthRequest.getInstance(url, "POST").body(requestData);
        req.doRequest();
        if (req.getStatus() != 200) {
            throw new CredentialException(req.getStatus(),
                    "Authentication error: " + req.getMessageFromJsonErrorMessage("error_message"));
        }
    }

    /**
     * Attempt to create an auth session for username/password. If {@code reuseExisting} is true then will first
     * try to use a pre-existing session if it's still valid. Otherwise this creates a new user session.
     *
     * @throws CredentialException if username/password are invalid
     */
    public void getSession(String username, String password, boolean reuseExisting) throws CredentialException {
        String sessionId = reuseExisting ? loadCookieData(username) : null;
        String requestData = getRequestData(username, password);
        AuthRequest req;

        if (sessionId != null) {
            String url = authUrl + "/session/" + sessionId +".json?appkey=" + appKey;
            req = AuthRequest.getInstance(url, "GET");
        } else {
            String url = authUrl + "/session.json?appkey=" + appKey;
            req = AuthRequest.getInstance(url, "POST").body(requestData);
        }

        req.doRequest();
        if (req.getStatus() == 200) {
            Gson gson = new Gson();
            AuthSessionData sessionData = gson.fromJson(req.getResponse(), AuthSessionData.class);
            sessionId = sessionData.id;
            key = sessionData.custom_params.get("key");
            secret = sessionData.custom_params.get("secret");
            saveCookieData(sessionId, username);
        } else if ((sessionId != null) && (req.getStatus() == 404)) {
            // Probably session expired, lets try again and create new one
            getSession(username, password, false);
        } else {
            removeCookieData();
            throw new CredentialException(req.getStatus(),
                    "Authentication error: " + req.getMessageFromJsonErrorMessage("error_message"));
        }
    }

    private String getRequestData(String username, String password) {
        Map<String, String> requestDataMap = new HashMap<>();
        Gson gson = new Gson();

        requestDataMap.put("password", password);
        if (Utils.isEmail(username)) {
            requestDataMap.put("email", username);
        } else {
            requestDataMap.put("username", username);
        }

        return gson.toJson(requestDataMap);
    }

    private boolean initializeCookieDir() {
		if (cookieDir != null) return true;
		if (maybeSetCookieDir(System.getProperty("user.home"))) return true;
		if (maybeSetCookieDir(System.getProperty("semantria.session"))) return true;
		if (maybeSetCookieDir("/tmp")) return true;
		if (maybeSetCookieDir("/temp")) return true;
		log.debug("No writeable directory found for session data. Will not cache session data.");
        return false;
    }

    private boolean maybeSetCookieDir(String name) {
        if (name == null) {
            return false;
        }
        File dir = new File(name);
        if (dir.isDirectory() && dir.canWrite()) {
			cookieDir = dir;
			return true;
		}
		return false;
    }

    private String loadCookieData(String username) {
		if (! initializeCookieDir()) {
			return null;
		}
        try {
            File sessionFile = new File(cookieDir, cookieFileName);
            if (sessionFile.exists()) {
                String sessionData = new String(Files.readAllBytes(sessionFile.toPath()));
                if (Strings.isNullOrEmpty(sessionData)) {
                    return null;
                }
                String[] parts = sessionData.split("\n");
                if ((parts.length < 2) || (! username.equals(parts[0].trim()))) {
                    return null;
                }
                return parts[1].trim();
            }
        } catch (IOException e) {
			log.debug("Error reading session data", e);
        }

        return null;
    }

    private void saveCookieData(String sessionId, String username) {
        if (! initializeCookieDir()) {
            return;
        }
        try {
            String sessionData = String.format("%s\n%s\n", username, sessionId);
            File sessionFile = new File(cookieDir, cookieFileName);
            if (!sessionFile.exists()) {
                sessionFile.createNewFile();
            }
            Files.write(sessionFile.toPath(), sessionData.getBytes(), StandardOpenOption.TRUNCATE_EXISTING);
        } catch (IOException e) {
            log.debug("Error writing session data", e);
        }
    }

    private void removeCookieData() {
        try {
            File sessionFile = new File(cookieDir, cookieFileName);
            Files.deleteIfExists(sessionFile.toPath());
        } catch (IOException e) {
            log.debug("Error deleting session data", e);
        }
    }
}
