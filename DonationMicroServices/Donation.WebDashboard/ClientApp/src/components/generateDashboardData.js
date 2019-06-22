
export function generate2DigitNumber(n) {
    if (n < 10)
        return `0${n}`;
    return `${n}`;
}

export function generateTimeStamp(hour, minute) {
    return `${generate2DigitNumber(hour)}:${generate2DigitNumber(minute)}`;
}

function getRandomInt(max, min) {

    const v = Math.floor(Math.random() * Math.floor(max));
    if (v < min)
        return min;
    return v;
}

export function getDonationProcessedPerSecChartData() {
    const data = [];
    for (let i = 0; i < 60; i += 2) {
        data.push({ timeStamp: generateTimeStamp(14, i), donationProcessedPerSec: getRandomInt(120, 80) });
    }
    return data;
}

export function getDonationReceivedPerSecChartData() {
    const data = [];
    let donatioReceivedPerSec = 0;
    for (let i = 0; i < 60; i += 2) {
        donatioReceivedPerSec += getRandomInt(100, 1);
        data.push({ timeStamp: generateTimeStamp(14, i), donatioReceivedPerSec });
    }
    return data;
}
