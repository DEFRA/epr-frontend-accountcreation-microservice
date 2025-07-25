﻿const IDLE_TIMEOUT_MS = 17 * 60 * 1000; // 17 minutes before showing modal
const LOGOUT_COUNTDOWN_DURATION = 120; // Logout countdown starts from 120 seconds
const LOGOUT_REDIRECT_URL = "/create-account/Account/SessionSignOut";

let idleTimeout, logoutCountdown;

const SessionTimeoutManager = {
    idleDuration: IDLE_TIMEOUT_MS,
    countdownDuration: LOGOUT_COUNTDOWN_DURATION,
    uiElements: {},

    initialize() {
        const excludedPaths = ["/create-account/timeout-signed-out", "/create-account/signed-out"];
        if (excludedPaths.includes(window.location.pathname)) {
            return;
        }

        this.cacheDOMElements();
        this.resetIdleTimer();
        this.addEventListeners();
    },

    cacheDOMElements() {
        this.uiElements = {
            overlay: document.getElementById("overlay"),
            modal: document.getElementById("session-timeout-container-id"),
            continueBtn: document.getElementById("continueButton"),
            countdownTimer: document.getElementById("counterTimeout"),
            minutesLabel: document.getElementById("counterTimeout-minutes"),
            secondsLabel: document.getElementById("counterTimeout-seconds"),
        };
    },

    resetIdleTimer() {
        clearTimeout(idleTimeout);
        idleTimeout = setTimeout(() => this.showSessionTimeoutModal(), this.idleDuration);
    },

    showSessionTimeoutModal() {
        this.toggleModalVisibility(true);
        this.startLogoutCountdown();
    },

    toggleModalVisibility(isVisible) {
        const { modal, overlay } = this.uiElements;
        [modal, overlay].forEach(el => el && (el.style.display = isVisible ? "block" : "none"));
    },
    startLogoutCountdown() {
        const endTime = Date.now() + this.countdownDuration * 1000;

        clearInterval(logoutCountdown);
        logoutCountdown = setInterval(() => {
            const timeRemaining = Math.max(0, Math.floor((endTime - Date.now()) / 1000));

            this.updateCountdownDisplay(timeRemaining);

            if (timeRemaining <= 0) {
                clearInterval(logoutCountdown);
                this.redirectToLogout();
            }
        }, 1000);
    },

    updateCountdownDisplay(timeLeft) {
        const { countdownTimer, minutesLabel, secondsLabel } = this.uiElements;

        if (!countdownTimer) return;

        if (timeLeft > 60) {
            countdownTimer.innerText = '2';
            minutesLabel.style.display = "inline";
            secondsLabel.style.display = "none";
        } else {
            countdownTimer.innerText = timeLeft;
            minutesLabel.style.display = "none";
            secondsLabel.style.display = "inline";
        }
    },

    redirectToLogout() {
        window.location.href = LOGOUT_REDIRECT_URL;
    },

    addEventListeners() {
        const { continueBtn } = this.uiElements;

        if (continueBtn) {
            continueBtn.addEventListener("click", () => this.extendSession());
        }

        document.addEventListener("visibilitychange", () => {
            if (!document.hidden) this.resetIdleTimer();
        });

        const debounce = (callback, delay) => {
            let timeout;
            return () => {
                clearTimeout(timeout);
                timeout = setTimeout(callback, delay);
            };
        };

        const handleUserActivity = debounce(() => this.resetIdleTimer(), 500);

        ["mousemove", "keydown", "scroll", "click", "touchstart"].forEach(event => {
            document.addEventListener(event, handleUserActivity, { passive: true });
        });
    },

    extendSession() {
        this.toggleModalVisibility(false);
        clearInterval(logoutCountdown);
        this.resetIdleTimer();
        window.location.reload();
    }
};

document.addEventListener("DOMContentLoaded", () => SessionTimeoutManager.initialize());
