CREATE TABLE health_check_history (
    id         INT AUTO_INCREMENT PRIMARY KEY,
    status     VARCHAR(20) NOT NULL,
    checked_at DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP
);
