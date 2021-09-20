const { open } = require('fs/promises')

const writeData = async () => {
    const startDate = new Date('2021-09-01')
    const endDate = new Date('2021-10-01')

    console.log('opening file output...')
    const file = await open('data.json', 'w')

    let prefix = '[\n  '
    let date = new Date(startDate.getTime() + Math.floor(Math.random() * 1000 * 10))
    console.log('generating data (this could take a while)...')
    while (date < endDate) {
        const userId = Math.floor(Math.random() * 100)
        const points = Math.floor(Math.random() * 100)
        const data = { time: date.toISOString(), userId: `user${userId}`, points: points }
        await file.write(prefix + JSON.stringify(data))
        prefix = ',\n  '
        date = new Date(date.getTime() + Math.floor(Math.random() * 1000 * 10))
    }

    await file.write('\n]\n')
    console.log('done.')
}

writeData()
