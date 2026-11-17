CREATE DATABASE IF NOT EXISTS gravity_simulator CHARACTER SET utf8mb4;
USE gravity_simulator;

CREATE TABLE IF NOT EXISTS simulacao (
    id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    criada_em DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    encerrada_em DATETIME(6) NULL,
    corpos_iniciais INT NOT NULL,
    iteracoes_previstas INT NOT NULL,
    ultima_iteracao INT NOT NULL DEFAULT 0,
    segundos_por_iteracao DOUBLE NOT NULL,
    INDEX idx_simulacao_encerrada (encerrada_em, id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS corpo_iteracao (
    simulacao_id BIGINT NOT NULL,
    iteracao INT NOT NULL,
    ordem SMALLINT NOT NULL,
    nome VARCHAR(64) NOT NULL,
    massa DOUBLE NOT NULL,
    densidade DOUBLE NOT NULL,
    pos_x DOUBLE NOT NULL,
    pos_y DOUBLE NOT NULL,
    vel_x DOUBLE NOT NULL,
    vel_y DOUBLE NOT NULL,
    volume DOUBLE NOT NULL,
    raio DOUBLE NOT NULL,
    PRIMARY KEY (simulacao_id, iteracao, ordem),
    CONSTRAINT fk_corpo_simulacao FOREIGN KEY (simulacao_id)
        REFERENCES simulacao (id) ON DELETE CASCADE
) ENGINE=InnoDB;

DROP PROCEDURE IF EXISTS excluir_simulacoes_antigas;
DELIMITER //
CREATE PROCEDURE excluir_simulacoes_antigas()
BEGIN
    DECLARE removidas INT DEFAULT 1;
    DECLARE limite DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6) - INTERVAL 1 MONTH;

    -- Cada lote confirma a exclusão de até cinco simulações e seus quadros.
    WHILE removidas > 0 DO
        START TRANSACTION;
        DELETE FROM simulacao
        WHERE encerrada_em < limite
        ORDER BY encerrada_em, id
        LIMIT 5;
        SET removidas = ROW_COUNT();
        COMMIT;
    END WHILE;
END//
DELIMITER ;

CREATE EVENT IF NOT EXISTS limpar_simulacoes_diariamente
    ON SCHEDULE EVERY 1 DAY STARTS CURRENT_TIMESTAMP + INTERVAL 1 DAY
    DO CALL excluir_simulacoes_antigas();

SET GLOBAL event_scheduler = ON;
