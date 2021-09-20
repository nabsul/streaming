const fname = 'data.json'
const numTags = 100
const startDate = new Date('2021-09-01')
const endDate = new Date('2021-10-01')

const { open } = require('fs/promises')

const generateEntry = (date) => {
    const userId = Math.floor(Math.random() * numTags)
    const val = Math.floor(Math.random() * 100)

    return {
        time: date.toISOString(),
        userId: `user${userId}`,
        points: val
    }
}

const incrementDate = (date) => new Date(date.getTime() + Math.floor(Math.random() * 1000 * 10))

const writeData = async () => {
    console.log('opening file output...')
    var file = await open(fname, 'w')

    console.log('generating data (this could take a while)...')
    await file.write('[')

    let date = incrementDate(startDate)
    let first = true
    while (date < endDate) {
        if (first) {
            first = false
            await file.write('\n\t')
        } else {
            await file.write(',\n\t')
        }

        await file.write(JSON.stringify(generateEntry(date)))
        date = incrementDate(date)
    }

    await file.write('\n]')

    console.log('done.')
    await file.close()
}

writeData()
