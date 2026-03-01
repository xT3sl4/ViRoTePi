-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Mar 01, 2026 at 09:51 PM
-- Wersja serwera: 10.4.32-MariaDB
-- Wersja PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `inpost`
--

-- --------------------------------------------------------

--
-- Struktura tabeli dla tabeli `admins`
--

CREATE TABLE `admins` (
  `admin_id` int(11) NOT NULL,
  `user_id` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_polish_ci;

--
-- Dumping data for table `admins`
--

INSERT INTO `admins` (`admin_id`, `user_id`) VALUES
(10, 1),
(11, 2);

-- --------------------------------------------------------

--
-- Struktura tabeli dla tabeli `boxs`
--

CREATE TABLE `boxs` (
  `box_id` int(11) NOT NULL,
  `pack_id` int(11) DEFAULT NULL,
  `size` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_polish_ci;

--
-- Dumping data for table `boxs`
--

INSERT INTO `boxs` (`box_id`, `pack_id`, `size`) VALUES
(1, 1, 'S'),
(2, 2, 'M'),
(3, 3, 'L'),
(4, 4, 'S'),
(5, 5, 'M'),
(6, 6, 'L'),
(7, 7, 'S'),
(8, 8, 'M'),
(9, 9, 'L'),
(10, 10, 'S');

-- --------------------------------------------------------

--
-- Struktura tabeli dla tabeli `klients`
--

CREATE TABLE `klients` (
  `klient_id` int(11) NOT NULL,
  `user_id` int(11) DEFAULT NULL,
  `pack_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_polish_ci;

--
-- Dumping data for table `klients`
--

INSERT INTO `klients` (`klient_id`, `user_id`, `pack_id`) VALUES
(1, 1, 0),
(2, 6, 0),
(3, 7, 0),
(4, 8, 0),
(5, 10, 0);

-- --------------------------------------------------------

--
-- Struktura tabeli dla tabeli `kuriers`
--

CREATE TABLE `kuriers` (
  `kurier_id` int(11) NOT NULL,
  `user_id` int(11) DEFAULT NULL,
  `state` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_polish_ci;

--
-- Dumping data for table `kuriers`
--

INSERT INTO `kuriers` (`kurier_id`, `user_id`, `state`) VALUES
(1, 3, 'available'),
(2, 4, 'busy'),
(3, 9, 'available');

-- --------------------------------------------------------

--
-- Struktura tabeli dla tabeli `kuriers_data`
--

CREATE TABLE `kuriers_data` (
  `kurier_id` int(11) DEFAULT NULL,
  `pack_id` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_polish_ci;

--
-- Dumping data for table `kuriers_data`
--

INSERT INTO `kuriers_data` (`kurier_id`, `pack_id`) VALUES
(1, 1),
(1, 2),
(1, 7),
(1, 10),
(2, 3),
(2, 4),
(2, 8),
(3, 5),
(3, 6),
(3, 9);

-- --------------------------------------------------------

--
-- Struktura tabeli dla tabeli `packs`
--

CREATE TABLE `packs` (
  `pack_id` int(11) NOT NULL,
  `size` text DEFAULT NULL,
  `klient_id` int(11) DEFAULT NULL,
  `delivered` tinyint(1) DEFAULT NULL,
  `date` date DEFAULT NULL,
  `to_when` date DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_polish_ci;

--
-- Dumping data for table `packs`
--

INSERT INTO `packs` (`pack_id`, `size`, `klient_id`, `delivered`, `date`, `to_when`) VALUES
(1, 'S', 1, 0, '2025-11-01', '2025-11-05'),
(2, 'M', 2, 1, '2025-11-01', '2025-11-04'),
(3, 'L', 3, 0, '2025-11-02', '2025-11-06'),
(4, 'S', 4, 1, '2025-11-02', '2025-11-03'),
(5, 'M', 5, 0, '2025-11-03', '2025-11-07'),
(6, 'L', 1, 0, '2025-11-03', '2025-11-08'),
(7, 'S', 2, 1, '2025-11-04', '2025-11-06'),
(8, 'M', 3, 0, '2025-11-04', '2025-11-09'),
(9, 'L', 4, 1, '2025-11-05', '2025-11-10'),
(10, 'S', 5, 0, '2025-11-05', '2025-11-11');

-- --------------------------------------------------------

--
-- Struktura tabeli dla tabeli `paczkomat`
--

CREATE TABLE `paczkomat` (
  `paczkomat_id` int(11) NOT NULL,
  `paczkomat_name` text DEFAULT NULL,
  `address` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_polish_ci;

--
-- Dumping data for table `paczkomat`
--

INSERT INTO `paczkomat` (`paczkomat_id`, `paczkomat_name`, `address`) VALUES
(1, 'GDA01', 'Gdańsk ul. Długa 1'),
(2, 'GDA02', 'Gdańsk ul. Grunwaldzka 15'),
(3, 'GDA03', 'Gdańsk ul. Kartuska 22'),
(4, 'GDA04', 'Gdańsk ul. Marynarki Polskiej 45'),
(5, 'GDA05', 'Gdańsk ul. Chłopska 10'),
(6, 'GDA06', 'Gdańsk ul. Kołobrzeska 33'),
(7, 'GDA07', 'Gdańsk ul. Piastowska 8'),
(8, 'GDA08', 'Gdańsk ul. Świętokrzyska 50'),
(9, 'GDA09', 'Gdańsk ul. Hallera 120'),
(10, 'GDA10', 'Gdańsk ul. Słowackiego 90');

-- --------------------------------------------------------

--
-- Struktura tabeli dla tabeli `paczkomat_data`
--

CREATE TABLE `paczkomat_data` (
  `paczkomat_id` int(11) DEFAULT NULL,
  `box_id` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_polish_ci;

--
-- Dumping data for table `paczkomat_data`
--

INSERT INTO `paczkomat_data` (`paczkomat_id`, `box_id`) VALUES
(1, 1),
(2, 2),
(3, 3),
(4, 4),
(5, 5),
(6, 6),
(7, 7),
(8, 8),
(9, 9),
(10, 10);

-- --------------------------------------------------------

--
-- Struktura tabeli dla tabeli `users`
--

CREATE TABLE `users` (
  `id` int(11) NOT NULL,
  `name` text DEFAULT NULL,
  `surname` text DEFAULT NULL,
  `email` text DEFAULT NULL,
  `date_of_birth` date DEFAULT NULL,
  `password` text DEFAULT NULL,
  `role` text DEFAULT NULL,
  `phone_number` int(11) DEFAULT NULL,
  `selfie` text DEFAULT NULL,
  `sex` tinyint(1) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_polish_ci;

--
-- Dumping data for table `users`
--

INSERT INTO `users` (`id`, `name`, `surname`, `email`, `date_of_birth`, `password`, `role`, `phone_number`, `selfie`, `sex`) VALUES
(1, 'Natalia', 'Dąbrowska', 'natalia10@mail.pl', '1994-10-10', 'pass123', 'klient', 101010101, 'selfie10.jpg', 0),
(2, 'Jan', 'Kowalski', 'jan1@mail.pl', '1990-01-01', 'pass123', 'admin', 111111111, 'selfie1.jpg', 1),
(3, 'Anna', 'Nowak', 'anna2@mail.pl', '1992-02-02', 'pass123', 'admin', 222222222, 'selfie2.jpg', 0),
(4, 'Piotr', 'Wiśniewski', 'piotr3@mail.pl', '1995-03-03', 'pass123', 'kurier', 333333333, 'selfie3.jpg', 1),
(5, 'Kasia', 'Wójcik', 'kasia4@mail.pl', '1996-04-04', 'pass123', 'kurier', 444444444, 'selfie4.jpg', 0),
(6, 'Tomasz', 'Kamiński', 'tomasz5@mail.pl', '1988-05-05', 'pass123', 'klient', 555555555, 'selfie5.jpg', 1),
(7, 'Magda', 'Lewandowska', 'magda6@mail.pl', '1999-06-06', 'pass123', 'klient', 666666666, 'selfie6.jpg', 0),
(8, 'Adam', 'Zieliński', 'adam7@mail.pl', '1991-07-07', 'pass123', 'klient', 777777777, 'selfie7.jpg', 1),
(9, 'Ola', 'Szymańska', 'ola8@mail.pl', '1993-08-08', 'pass123', 'klient', 888888888, 'selfie8.jpg', 0),
(10, 'Marek', 'Woźniak', 'marek9@mail.pl', '1987-09-09', 'pass123', 'kurier', 999999999, 'selfie9.jpg', 1);

--
-- Indeksy dla zrzutów tabel
--

--
-- Indeksy dla tabeli `admins`
--
ALTER TABLE `admins`
  ADD PRIMARY KEY (`admin_id`),
  ADD KEY `user_id` (`user_id`);

--
-- Indeksy dla tabeli `boxs`
--
ALTER TABLE `boxs`
  ADD PRIMARY KEY (`box_id`),
  ADD KEY `pack_id` (`pack_id`);

--
-- Indeksy dla tabeli `klients`
--
ALTER TABLE `klients`
  ADD PRIMARY KEY (`klient_id`),
  ADD KEY `user_id` (`user_id`);

--
-- Indeksy dla tabeli `kuriers`
--
ALTER TABLE `kuriers`
  ADD PRIMARY KEY (`kurier_id`),
  ADD KEY `user_id` (`user_id`);

--
-- Indeksy dla tabeli `kuriers_data`
--
ALTER TABLE `kuriers_data`
  ADD KEY `kurier_id` (`kurier_id`,`pack_id`),
  ADD KEY `pack_id` (`pack_id`);

--
-- Indeksy dla tabeli `packs`
--
ALTER TABLE `packs`
  ADD PRIMARY KEY (`pack_id`),
  ADD KEY `klient_id` (`klient_id`);

--
-- Indeksy dla tabeli `paczkomat`
--
ALTER TABLE `paczkomat`
  ADD PRIMARY KEY (`paczkomat_id`);

--
-- Indeksy dla tabeli `paczkomat_data`
--
ALTER TABLE `paczkomat_data`
  ADD KEY `paczkomat_id` (`paczkomat_id`,`box_id`),
  ADD KEY `box_id` (`box_id`);

--
-- Indeksy dla tabeli `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `admins`
--
ALTER TABLE `admins`
  MODIFY `admin_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=12;

--
-- AUTO_INCREMENT for table `boxs`
--
ALTER TABLE `boxs`
  MODIFY `box_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT for table `klients`
--
ALTER TABLE `klients`
  MODIFY `klient_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT for table `kuriers`
--
ALTER TABLE `kuriers`
  MODIFY `kurier_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `packs`
--
ALTER TABLE `packs`
  MODIFY `pack_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT for table `paczkomat`
--
ALTER TABLE `paczkomat`
  MODIFY `paczkomat_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT for table `users`
--
ALTER TABLE `users`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=12;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `admins`
--
ALTER TABLE `admins`
  ADD CONSTRAINT `admins_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `boxs`
--
ALTER TABLE `boxs`
  ADD CONSTRAINT `boxs_ibfk_1` FOREIGN KEY (`pack_id`) REFERENCES `packs` (`pack_id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `klients`
--
ALTER TABLE `klients`
  ADD CONSTRAINT `klients_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `kuriers`
--
ALTER TABLE `kuriers`
  ADD CONSTRAINT `kuriers_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `kuriers_data`
--
ALTER TABLE `kuriers_data`
  ADD CONSTRAINT `kuriers_data_ibfk_1` FOREIGN KEY (`pack_id`) REFERENCES `packs` (`pack_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `kuriers_data_ibfk_2` FOREIGN KEY (`kurier_id`) REFERENCES `kuriers` (`kurier_id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `packs`
--
ALTER TABLE `packs`
  ADD CONSTRAINT `packs_ibfk_1` FOREIGN KEY (`klient_id`) REFERENCES `klients` (`klient_id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `paczkomat_data`
--
ALTER TABLE `paczkomat_data`
  ADD CONSTRAINT `paczkomat_data_ibfk_1` FOREIGN KEY (`paczkomat_id`) REFERENCES `paczkomat` (`paczkomat_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `paczkomat_data_ibfk_2` FOREIGN KEY (`box_id`) REFERENCES `boxs` (`box_id`) ON DELETE CASCADE ON UPDATE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
